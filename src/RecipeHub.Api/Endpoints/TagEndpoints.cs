using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;
using RecipeHub.Api.Dtos;

namespace RecipeHub.Api.Endpoints;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tags").WithTags("Tags");

        group.MapGet("/", GetAllAsync);
    }

    private static async Task<IResult> GetAllAsync(RecipeDbContext db, CancellationToken ct)
    {
        var tags = await db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.RecipeTags.Count))
            .ToListAsync(ct);

        return Results.Ok(tags);
    }
}
