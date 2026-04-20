namespace RecipeHub.Api.Endpoints;

public static class FavoriteEndpoints
{
    private const string NotImplementedMessage =
        "Favorites feature not yet implemented — Challenge 02 material";

    public static void MapFavoriteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/favorites").WithTags("Favorites");

        group.MapGet("/", () => Results.Json(new { message = NotImplementedMessage }, statusCode: 501));
        group.MapPost("/", () => Results.Json(new { message = NotImplementedMessage }, statusCode: 501));
        group.MapDelete("/{recipeId:int}", (int recipeId) =>
            Results.Json(new { message = NotImplementedMessage }, statusCode: 501));
    }
}
