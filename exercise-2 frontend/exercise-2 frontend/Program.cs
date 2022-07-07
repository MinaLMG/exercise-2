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
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public List<Guid> Categories { get; set; }
        public Guid ID { get; set; }

        public Recipe()
        {

        }

        public Recipe(string title, string ingredients, string instructions, List<Guid> categories)
        {
            this.Title = title;
            this.Ingredients = ingredients;
            this.Instructions = instructions;
            this.Categories = categories;
            this.ID = Guid.NewGuid();
        }

        public string Display(Dictionary<Guid, string> categoriesNamesMap)
        {
            string toDisplay = "this receipe is called :" + this.Title + ", to do it we need: " + this.Ingredients + ", the instructions are: " + this.Instructions;
            for (int i = 0; i < this.Categories.Count; i++)
            {
                if (i == 0)
                {
                    toDisplay += ", for categories it is considered as: ";
                }
                if (i != 0) { toDisplay += "       "; }
                toDisplay += categoriesNamesMap[this.Categories[i]];
            }
            toDisplay += "\n\n";
            return toDisplay;
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

    public static void WriteInFolder(string text, string path)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(text);
        }
    }

    public static string ListCategories(List<Category> categories)
    {
        string categoriesString = "";
        for (int i = 0; i < categories.Count; i++)
        {
            categoriesString += "at index ";
            categoriesString += i;
            categoriesString += " ";
            categoriesString += categories[i].Display();
            categoriesString += "\n\n";
        }
        return categoriesString;
    }

    public static string ListRecipes(List<Recipe> receipes, Dictionary<Guid, string> categoriesNamesMap)
    {
        string receipesString = "";
        for (int i = 0; i < receipes.Count; i++)
        {
            receipesString += "at index ";
            receipesString += i;
            receipesString += " ";
            receipesString += receipes[i].Display(categoriesNamesMap);
            receipesString += "\n\n";
        }
        return receipesString;
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
                                    var table = new Table();
                                    table.AddColumn("Title");
                                    table.AddColumn("Ingredients");
                                    table.AddColumn("Instructions");
                                    table.AddColumn("categoties");
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
                                        // Add some rows
                                        table.AddRow(new Markup(r.Title), new Markup(r.Ingredients), new Markup(r.Instructions), new Panel(categoriesTable));
                                    }
                                    AnsiConsole.Write(table);
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
                                    var title = AnsiConsole.Ask<string>("What's the recipe title?");
                                    var ingredients = AnsiConsole.Ask<string>("What's the recipe ingredients?");
                                    var instructions = AnsiConsole.Ask<string>("What's the recipe instructions?");
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
                                    Recipe to_add = new Recipe(title, ingredients, instructions, chosenCategoriesFinal);
                                    var temp = JsonSerializer.Serialize(to_add);
                                    var res3 = await httpClient.PostAsync("https://localhost:7131/recipes", new StringContent(temp, Encoding.UTF8, "application/json"));
                                    recipes.Add(to_add);
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
                                    var table = new Table();
                                    table.AddColumn("index");
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
                                        // Add some rows
                                        table.AddRow(new Markup(indexer.ToString()), new Markup(r.Title), new Markup(r.Ingredients), new Markup(r.Instructions), new Panel(categoriesTable));
                                        indexer++;
                                    }
                                    AnsiConsole.Write(table);
                                    var index = -1;
                                    while (index < 0 || index >= recipes.Count)
                                    {
                                        index = int.Parse(AnsiConsole.Ask<string>("choose an index to edit"));
                                    }
                                    var title = AnsiConsole.Ask<string>("What's the recipe new title?");
                                    var ingredients = AnsiConsole.Ask<string>("What's the recipe new ingredients?");
                                    var instructions = AnsiConsole.Ask<string>("What's the recipe new instructions?");
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
                                    Recipe toEdit = new Recipe(title, ingredients, instructions, chosenCategoriesFinal);
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
                                    var table = new Table();
                                    table.AddColumn("ID");
                                    table.AddColumn("Name");
                                    foreach (Category c in categories)
                                    {
                                        table.AddRow(c.ID.ToString(), c.Name);
                                    }
                                    AnsiConsole.Write(table);
                                    backChoice =  "Back" ;
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
                                    var name = AnsiConsole.Ask<string>("What's the category name?").ToLower().Trim();
                                    try
                                    {
                                        while (categoriesMap.ContainsKey(null))
                                        {
                                            name = AnsiConsole.Ask<string>("this category name already exists, Enter neew one please. ").ToLower().Trim();

                                        }
                                    }
                                    catch (Exception e) { }
                                    Category to_add = new Category(name);
                                    var temp = JsonSerializer.Serialize(to_add);
                                    var res3 = await httpClient.PostAsync("https://localhost:7131/categories", new StringContent(temp, Encoding.UTF8, "application/json"));
                                    categories.Add(to_add);
                                    categoriesMap[name] = to_add.ID;
                                    categoriesNamesMap[to_add.ID] = to_add.Name;
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
                                    var table = new Table();
                                    table.AddColumn("index");
                                    table.AddColumn("Name");
                                    int counter = 0;
                                    foreach (Category c in categories)
                                    {
                                        table.AddRow(counter.ToString(), c.Name);
                                        counter++;
                                    }
                                    AnsiConsole.Write(table);
                                    var index = -1;
                                    while (index < 0 || index >= categories.Count)
                                    {
                                        index = int.Parse(AnsiConsole.Ask<string>("choose an index to edit"));
                                    }
                                    categoriesMap.Remove(categories[index].Name);
                                    var name = AnsiConsole.Ask<string>("What's the category new name?").ToLower().Trim();
                                    try
                                    {
                                        while (categoriesMap.ContainsKey(name))
                                        {
                                            name = AnsiConsole.Ask<string>("this category name already exists, Enter neew one please. ").ToLower().Trim();
                                        }
                                    }
                                    catch (Exception e) { }
                                    var temp = JsonSerializer.Serialize(new Category(name));
                                    var res3 = await httpClient.PutAsync("https://localhost:7131/categories/" + categories[index].ID, new StringContent(temp, Encoding.UTF8, "application/json"));
                                    categories[index].Name = name;
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