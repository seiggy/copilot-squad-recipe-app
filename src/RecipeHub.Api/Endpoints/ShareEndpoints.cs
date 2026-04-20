using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;
using RecipeHub.Api.Dtos;
using RecipeHub.Api.Models;

namespace RecipeHub.Api.Endpoints;

public static class ShareEndpoints
{
    public static void MapShareEndpoints(this WebApplication app)
    {
        app.MapPost("/api/recipes/{id:int}/share", CreateShareAsync).WithTags("Share");
        app.MapGet("/api/shared/{token}", GetSharedAsync).WithTags("Share");
    }

    private static async Task<IResult> CreateShareAsync(
        int id,
        RecipeDbContext db,
        CancellationToken ct)
    {
        var recipe = await db.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        var share = new ShareToken
        {
            RecipeId = id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        db.ShareTokens.Add(share);
        await db.SaveChangesAsync(ct);

        share.Token = Guid.NewGuid().ToString("N");

        return Results.Ok(new ShareDto(share.Token, $"/shared/{share.Token}"));
    }

    private static async Task<IResult> GetSharedAsync(
        string token,
        RecipeDbContext db,
        CancellationToken ct)
    {
        var share = await db.ShareTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Token == token, ct);

        if (share is null)
        {
            return Results.NotFound();
        }

        if (share.ExpiresAt.HasValue && share.ExpiresAt.Value < DateTime.UtcNow)
        {
            return Results.NotFound();
        }

        var recipe = await db.Recipes
            .AsNoTracking()
            .Include(r => r.Steps)
            .Include(r => r.RecipeTags)
                .ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(r => r.Id == share.RecipeId, ct);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToDetailDto(recipe));
    }

    private static RecipeDetailDto ToDetailDto(Recipe r) => new(
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
            .ToArray(),
        r.Steps
            .OrderBy(s => s.StepNumber)
            .Select(s => new RecipeStepDto(s.StepNumber, s.Instruction, s.TimerMinutes))
            .ToArray(),
        r.CreatedAt,
        r.UpdatedAt
    );
}
