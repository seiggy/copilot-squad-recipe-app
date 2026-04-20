using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;
using RecipeHub.Api.Dtos;
using RecipeHub.Api.Models;

namespace RecipeHub.Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes/search").WithTags("Search");

        group.MapGet("/", SearchAsync);
    }

    private static async Task<IResult> SearchAsync(
        string? q,
        string? tag,
        RecipeDbContext db,
        CancellationToken ct)
    {
        var query = db.Recipes
            .AsNoTracking()
            .Include(r => r.RecipeTags)
                .ThenInclude(rt => rt.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            query = query.Where(r =>
                r.Title.Contains(needle) ||
                (r.Description != null && r.Description.Contains(needle)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagName = tag.Trim();
            query = query.Where(r => r.RecipeTags.Any(rt => rt.Tag != null && rt.Tag.Name == tagName));
        }

        var recipes = await query
            .OrderBy(r => r.Id)
            .ToListAsync(ct);

        var dtos = recipes.Select(ToSummaryDto).ToArray();
        return Results.Ok(dtos);
    }

    private static RecipeDto ToSummaryDto(Recipe r) => new(
        r.Id,
        r.Title,
        r.Description,
        r.Difficulty.ToString(),
        r.PrepTimeMinutes,
        r.CookTimeMinutes,
        r.Servings,
        r.ImageUrl,
        r.RecipeTags
            .Where(rt => rt.Tag is not null)
            .Select(rt => rt.Tag!.Name)
            .OrderBy(n => n)
            .ToArray()
    );
}
