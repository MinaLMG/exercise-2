using Spectre.Console;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;

public static class Program
{
    public class Category
    {
        public string Name { get; set; }
        public Guid ID { get; set; }

        public Category()
        {

        }

        public Category(string x)
        {
            this.Name = x;
            this.ID = Guid.NewGuid(); ;
        }

        public string Display()
        {
            return "this category has a name : " + this.Name;
        }
    }

    public class Recipe
    {
        public string Title { get; set; }
        public List<string> Ingredients { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
        public List<Guid> Categories { get; set; } = new();
        public Guid ID { get; set; }

        public Recipe()
        {

        }

        public Recipe(string title, List<string> ingredients, List<string> instructions, List<Guid> categories)
        {
            this.Title = title;
            this.Ingredients = ingredients;
            this.Instructions = instructions;
            this.Categories = categories;
            this.ID = Guid.NewGuid();
        }
    }

    public static string Select(string[] choices, string title = "")
    {
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title(title)
        .PageSize(10)
        .AddChoices(choices));
        return choice;
    }

    public static void ListRecipes(List<Recipe>recipes, Dictionary<Guid, string> categoriesNamesMap,bool showIndex) {
        var table = new Table();
        if(showIndex) table.AddColumn("index");
        table.AddColumn("Title");
        table.AddColumn("Ingredients");
        table.AddColumn("Instructions");
        table.AddColumn("categoties");
        int indexer = 0;

        foreach (Recipe r in recipes)
        {
            string categoriesTable = "";
            int counter = 0;
            foreach (Guid g in r.Categories)
            {
                counter++;
                categoriesTable += categoriesNamesMap[g];
                if (counter < r.Categories.Count)
                    categoriesTable += "\n";
            }
            string ingredients = "";
            int iCounter = 0;
            foreach (string ingredient in r.Ingredients)
            {
                iCounter++;
                ingredients += "-";
                ingredients += ingredient;
                if (iCounter != r.Ingredients.Count)
                {
                    ingredients += "\n";
                }
            }
            string instructions = "";
            iCounter = 0;
            foreach (string instruction in r.Instructions)
            {
                iCounter++;
                instructions += "-";
                instructions += instruction;
                if (iCounter != r.Instructions.Count)
                {
                    instructions += "\n";
                }
            }
            if(!showIndex) table.AddRow(new Markup(r.Title), new Markup(ingredients), new Markup(instructions), new Panel(categoriesTable));
            else table.AddRow(new Markup(indexer.ToString()), new Markup(r.Title), new Markup(ingredients), new Markup(instructions), new Panel(categoriesTable));
            indexer++;
        }
        AnsiConsole.Write(table);
    }

    public static void ListCategories(List<Category>categories,bool showIndex )
    {
        var table = new Table();
        table.AddColumn(showIndex? "index":"ID");
        table.AddColumn("Name");
        int counter = 0;
        foreach (Category c in categories)
        {
            table.AddRow(showIndex? counter.ToString(): c.ID.ToString(), c.Name);
            counter++;
        }
        AnsiConsole.Write(table);
    }

    public static Recipe getRecipeInput(List< Category> categories, Dictionary<string, Guid> categoriesMap,bool isEditing) {
        var title = AnsiConsole.Ask<string>(isEditing? "what's the recipe new title?": "What's the recipe title?").Trim();
        List<string> ingredients = new();
        var ingredient = "";
        ingredient = AnsiConsole.Ask<string>("What's the recipe ingredients?\n[green]enter them one bye one seperated by enter (type END to get to the next step)[/]").Trim();
        while (ingredient != "END")
        {
            ingredients.Add(ingredient);
            ingredient = AnsiConsole.Ask<string>("[green]enter another ingredient (type END to get to the next step)[/]").Trim();
        }
        List<string> instructions = new();
        var instruction = "";
        instruction = AnsiConsole.Ask<string>("What's the recipe instructions?\n[green]enter them one bye one seperated by enter (type END to get to the next step)[/]").Trim();
        while (instruction != "END")
        {
            instructions.Add(instruction);
            instruction = AnsiConsole.Ask<string>(" [green]enter another instruction (type END to get to the next step)[/]").Trim();
        }
        var categoryNames = categories.Select(x => x.Name).ToArray();
        var chosenCategories = AnsiConsole.Prompt(
             new MultiSelectionPrompt<string>()
                 .Title("What are your [green]the recipe categories[/]?")
                 .NotRequired()
                 .PageSize(10)
                 .InstructionsText(
                     "[grey](Press [blue]<space>[/] to toggle a category, " +
                     "[green]<enter>[/] to accept)[/]")
                 .AddChoices(categoryNames));
        List<Guid> chosenCategoriesFinal = new List<Guid> { };
        for (int i = 0; i < chosenCategories.Count; i++)
        {
            chosenCategoriesFinal.Add(categoriesMap[chosenCategories[i]]);
        }
        Recipe recipe = new Recipe(title, ingredients, instructions, chosenCategoriesFinal);
        return recipe; 

    }

    public static Category getCategoryInput(Dictionary<string, Guid> categoriesMap,bool isEditing)
    {
        var name = AnsiConsole.Ask<string>(isEditing ? "What's the category new name?" : "What's the category name?").ToLower().Trim();
        try
        {
            while (categoriesMap.ContainsKey(null))
            {
                name = AnsiConsole.Ask<string>("this category name already exists, Enter neew one please. ").ToLower().Trim();
            }
        }
        catch (Exception e) { }
        Category category = new Category(name);
        return category;
    }

    public static async Task Main(string[] args)
    {
        AnsiConsole.Write(new FigletText("Mena Lateaf").Centered().Color(Color.Grey));
        string mainMenuChoice = string.Empty;
        string categoryChoice = string.Empty;
        string recipeChoice = string.Empty;
        string backChoice = string.Empty;
        HttpClient httpClient = new HttpClient();

        var res = await httpClient.GetAsync("https://localhost:7131/categories");
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var inBetween = res.Content.ReadAsStringAsync().Result;
        var categories = JsonSerializer.Deserialize<List<Category>>(inBetween, serializeOptions);
        Dictionary<string, Guid> categoriesMap = new Dictionary<string, Guid>();
        Dictionary<Guid, string> categoriesNamesMap = new Dictionary<Guid, string>();
        for (int i = 0; i < categories.Count; i++)
        {
            categoriesMap[categories[i].Name] = categories[i].ID;
            categoriesNamesMap[categories[i].ID] = categories[i].Name;
        }
        var res2 = await httpClient.GetAsync("https://localhost:7131/recipes");

        var inBetween2 = res2.Content.ReadAsStringAsync().Result;
        var recipes = JsonSerializer.Deserialize<List<Recipe>>(inBetween2, serializeOptions);
        bool continueCode = true;
        while (continueCode)
        {
            switch (mainMenuChoice)
            {
                case "":
                    mainMenuChoice = Select(new[] { "Recipes", "Categories", "Close program" }, "which struct would you like to deal with ?");
                    break;
                case "Recipes":
                    switch (recipeChoice)
                    {
                        case "":
                            recipeChoice = Select(new[] { "List", "Add", "Edit", "Bact to main menu" }, "what would you like to do with recipes?");
                            break;
                        case "List":
                            switch (backChoice)
                            {
                                case "":
                                    ListRecipes(recipes, categoriesNamesMap,false);
                                    backChoice = Select(new[] { "Back" });
                                    break;
                                case "Back":
                                    backChoice = "";
                                    recipeChoice = "";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Add":
                            switch (backChoice)
                            {
                                case "":
                                    Recipe toAdd = getRecipeInput(categories, categoriesMap,false);
                                    var temp = JsonSerializer.Serialize(toAdd);
                                    var res3 = await httpClient.PostAsync("https://localhost:7131/recipes", new StringContent(temp, Encoding.UTF8, "application/json"));
                                    recipes.Add(toAdd);
                                    backChoice = "Back";
                                    break;
                                case "Back":
                                    backChoice = "";
                                    recipeChoice = "";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Edit":
                            switch (backChoice)
                            {
                                case "":
                                    ListRecipes(recipes, categoriesNamesMap, true);
                                    var index = -1;
                                    while (index < 0 || index >= recipes.Count)
                                    {
                                        index = int.Parse(AnsiConsole.Ask<string>("choose an index to edit"));
                                    }
                                    Recipe toEdit = getRecipeInput(categories,categoriesMap,true);
                                    var temp = JsonSerializer.Serialize(toEdit);
                                    var res3 = await httpClient.PutAsync("https://localhost:7131/recipes/" + recipes[index].ID, new StringContent(temp, Encoding.UTF8, "application/json"));
                                    var inBetween3 = res3.Content.ReadAsStringAsync().Result;
                                    var recipeToEdit = JsonSerializer.Deserialize<Recipe>(inBetween3, serializeOptions);
                                    recipes[index] = recipeToEdit;
                                    backChoice = "Back";
                                    break;
                                case "Back":
                                    backChoice = "";
                                    recipeChoice = "";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Bact to main menu":
                            mainMenuChoice = "";
                            recipeChoice = "";
                            break;
                        default:
                            break;
                    }
                    break;
                case "Categories":
                    switch (categoryChoice)
                    {
                        case "":
                            categoryChoice = Select(new[] { "List", "Add", "Edit", "Bact to main menu" }, "what would you like to do with categories?");
                            break;
                        case "List":
                            switch (backChoice)
                            {
                                case "":
                                    ListCategories(categories,false);
                                    backChoice = "Back";
                                    break;
                                case "Back":
                                    backChoice = "";
                                    categoryChoice = "";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Add":
                            switch (backChoice)
                            {
                                case "":
                                    Category toAdd = getCategoryInput(categoriesMap, false);
                                    var temp = JsonSerializer.Serialize(toAdd);
                                    var res3 = await httpClient.PostAsync("https://localhost:7131/categories", new StringContent(temp, Encoding.UTF8, "application/json"));
                                    categories.Add(toAdd);
                                    categoriesMap[toAdd.Name] = toAdd.ID;
                                    categoriesNamesMap[toAdd.ID] = toAdd.Name;
                                    backChoice = "Back";
                                    break;
                                case "Back":
                                    backChoice = "";
                                    categoryChoice = "";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Edit":
                            switch (backChoice)
                            {
                                case "":
                                    ListCategories(categories, true);
                                    var index = -1;
                                    while (index < 0 || index >= categories.Count)
                                    {
                                        index = int.Parse(AnsiConsole.Ask<string>("choose an index to edit"));
                                    }
                                    categoriesMap.Remove(categories[index].Name);
                                   Category toEdit=getCategoryInput(categoriesMap, true);
                                    var temp = JsonSerializer.Serialize(toEdit);
                                    var res3 = await httpClient.PutAsync("https://localhost:7131/categories/" + categories[index].ID, new StringContent(temp, Encoding.UTF8, "application/json"));
                                    categories[index].Name = toEdit.Name;
                                    categoriesMap[categories[index].Name] = categories[index].ID;
                                    categoriesNamesMap[categories[index].ID] = categories[index].Name;
                                    backChoice = "Back";
                                    break;
                                case "Back":
                                    backChoice = "";
                                    categoryChoice = "";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Bact to main menu":
                            mainMenuChoice = "";
                            categoryChoice = "";
                            break;
                        default:
                            break;
                    }
                    break;
                case "Close program":
                    continueCode = false;
                    break;
                default:
                    break;
            }
        }
    }
}