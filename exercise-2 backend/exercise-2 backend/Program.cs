using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


var app = WebApplication.Create();
Data data = new Data();
Pages pages = new Pages(data);
pages.CategoryPages(app);
pages.RecipePages(app);
app.Run();

public class Data
{
    public List<Category> Categories { get; set; }
    public List<Recipe> Recipes { get; set; }

    public Dictionary<string, Guid> CategoriesMap { get; set; }
    public Dictionary<Guid, string> CategoriesNamesMap { get; set; }

    public string RecipesLoc { get; set; }
    public string CategoriesLoc { get; set; }

    public JsonSerializerOptions Options { get; set; }


    public void WriteInFolder(string text, string path)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine(text);
        }
    }

    public Data()
    {
        this.Options = new JsonSerializerOptions { WriteIndented = true };
        string mainPath = Environment.CurrentDirectory;
        this.CategoriesLoc = $@"{mainPath}\..\categories.json";
        string categoriesString = File.ReadAllText(this.CategoriesLoc);
        this.Categories = JsonSerializer.Deserialize<List<Category>>(categoriesString);
        /****/
        this.CategoriesMap = new Dictionary<string, Guid>();
        this.CategoriesNamesMap = new Dictionary<Guid, string>();
        for (int i = 0; i < this.Categories.Count; i++)
        {
            this.CategoriesMap[this.Categories[i].Name] = this.Categories[i].ID;
            this.CategoriesNamesMap[this.Categories[i].ID] = this.Categories[i].Name;
        }
        this.RecipesLoc = $@"{mainPath}\..\recipes.json";
        string recipesString = File.ReadAllText(this.RecipesLoc);
        this.Recipes = JsonSerializer.Deserialize<List<Recipe>>(recipesString);
    }
    public void AddCategory(Category to_add)
    {
        this.Categories.Add(to_add);
        this.CategoriesMap[to_add.Name] = to_add.ID;
        this.CategoriesNamesMap[to_add.ID] = to_add.Name;
        this.WriteInFolder(JsonSerializer.Serialize(this.Categories, this.Options), this.CategoriesLoc);
    }
    public Category EditCategory(Guid id, Category newCategory)
    {
        Category to_edit = this.Categories.Single(x => x.ID == id);
        to_edit.Name = newCategory.Name;
        this.WriteInFolder(JsonSerializer.Serialize(this.Categories, this.Options), this.CategoriesLoc);
        return to_edit;
    }
    public Recipe EditRecipe(Guid id, Recipe newRecipe)
    {

        Recipe to_edit = this.Recipes.Single(x => x.ID == id);
        to_edit.Title = newRecipe.Title;
        to_edit.Instructions = newRecipe.Instructions;
        to_edit.Ingredients = newRecipe.Ingredients;
        to_edit.Categories = newRecipe.Categories;
        this.WriteInFolder(JsonSerializer.Serialize(this.Recipes, this.Options), this.RecipesLoc);
        return to_edit;
    }
    public void AddRecipe(Recipe to_add)
    {
        this.Recipes.Add(to_add);
        this.WriteInFolder(JsonSerializer.Serialize(this.Recipes, this.Options), this.RecipesLoc);
    }
}
public class Pages
{
    public Data data { get; set; }
    public IResult CreateCategory([FromBody] Category c)
    {
        data.AddCategory(c);
        return Results.Json(new { c.Name, c.ID });
    }
    public IResult EditCategory(Guid id, [FromBody] Category c)
    {
        Console.WriteLine("here");
        Category to_edit = data.EditCategory(id, c);
        return Results.Json(to_edit);
    }
    public IResult EditRecipe(Guid id, [FromBody] Recipe r)
    {
        Console.WriteLine("here");
        Recipe to_edit = data.EditRecipe(id, r);
        return Results.Json(to_edit);
    }
    public IResult CreateRecipe([FromBody] Recipe r)
    {
        data.AddRecipe(r);
        return Results.Json(new { r.Title, r.Ingredients, r.Instructions, r.Categories, r.ID });
    }

    public void CategoryPages(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/categories", () => Results.Json(data.Categories));
        endpoints.MapPost("/categories", CreateCategory);
        endpoints.MapPut("/categories/{id}", EditCategory);
    }
    public void RecipePages(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/recipes", () => Results.Json(data.Recipes));
        endpoints.MapPost("/recipes", CreateRecipe);
        endpoints.MapPut("/recipes/{id}", EditRecipe);
    }
    public Pages(Data data) { this.data = data; }
}
public class Category
{
    public string Name { get; set; }
    public Guid ID { get; set; }
}
public class Recipe
{
    public string Title { get; set; }
    public string Ingredients { get; set; }
    public string Instructions { get; set; }
    public List<Guid> Categories { get; set; }
    public Guid ID { get; set; }

}