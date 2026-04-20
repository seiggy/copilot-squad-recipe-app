using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;
using RecipeHub.Api.Dtos;
using RecipeHub.Api.Models;

namespace RecipeHub.Api.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes").WithTags("Recipes");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(RecipeDbContext db, CancellationToken ct)
    {
        var recipes = await db.Recipes
            .AsNoTracking()
            .Include(r => r.RecipeTags)
                .ThenInclude(rt => rt.Tag)
            .OrderBy(r => r.Id)
            .ToListAsync(ct);

        var dtos = recipes.Select(ToSummaryDto).ToArray();
        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetByIdAsync(int id, RecipeDbContext db, CancellationToken ct)
    {
        var recipe = await db.Recipes
            .AsNoTracking()
            .Include(r => r.Steps)
            .Include(r => r.RecipeTags)
                .ThenInclude(rt => rt.Tag)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return recipe is null ? Results.NotFound() : Results.Ok(ToDetailDto(recipe));
    }

    private static async Task<IResult> CreateAsync(
        CreateRecipeRequest request,
        RecipeDbContext db,
        CancellationToken ct)
    {
        if (!TryParseDifficulty(request.Difficulty, out var difficulty))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["difficulty"] = [$"Invalid difficulty value '{request.Difficulty}'. Expected Easy, Medium, or Hard."]
            });
        }

        var now = DateTime.UtcNow;
        var recipe = new Recipe
        {
            Title = request.Title,
            Description = request.Description,
            Difficulty = difficulty,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            ImageUrl = request.ImageUrl,
            CreatedAt = now,
            UpdatedAt = now,
            Steps = request.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => new RecipeStep
                {
                    StepNumber = s.StepNumber,
                    Instruction = s.Instruction,
                    TimerMinutes = s.TimerMinutes
                })
                .ToList()
        };

        var tagLinks = await BuildTagLinksAsync(db, request.Tags, ct);
        foreach (var link in tagLinks)
        {
            recipe.RecipeTags.Add(link);
        }

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(ct);

        await db.Entry(recipe).Collection(r => r.RecipeTags).Query().Include(rt => rt.Tag).LoadAsync(ct);

        var dto = ToDetailDto(recipe);
        return Results.Created($"/api/recipes/{recipe.Id}", dto);
    }

    private static async Task<IResult> UpdateAsync(
        int id,
        UpdateRecipeRequest request,
        RecipeDbContext db,
        CancellationToken ct)
    {
        var recipe = await db.Recipes
            .Include(r => r.Steps)
            .Include(r => r.RecipeTags)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        if (!TryParseDifficulty(request.Difficulty, out var difficulty))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["difficulty"] = [$"Invalid difficulty value '{request.Difficulty}'. Expected Easy, Medium, or Hard."]
            });
        }

        recipe.Title = request.Title;
        recipe.Description = request.Description;
        recipe.Difficulty = difficulty;
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.ImageUrl = request.ImageUrl;
        recipe.UpdatedAt = DateTime.UtcNow;

        db.RecipeSteps.RemoveRange(recipe.Steps);
        recipe.Steps = request.Steps
            .OrderBy(s => s.StepNumber)
            .Select(s => new RecipeStep
            {
                RecipeId = recipe.Id,
                StepNumber = s.StepNumber,
                Instruction = s.Instruction,
                TimerMinutes = s.TimerMinutes
            })
            .ToList();

        recipe.RecipeTags.Clear();
        var tagLinks = await BuildTagLinksAsync(db, request.Tags, ct);
        foreach (var link in tagLinks)
        {
            recipe.RecipeTags.Add(link);
        }

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(int id, RecipeDbContext db, CancellationToken ct)
    {
        var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (recipe is null)
        {
            return Results.NotFound();
        }

        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<List<RecipeTag>> BuildTagLinksAsync(
        RecipeDbContext db,
        string[] tagNames,
        CancellationToken ct)
    {
        if (tagNames is null || tagNames.Length == 0)
        {
            return new List<RecipeTag>();
        }

        // Match-only strategy: join existing tags by Name; unknown names are skipped silently.
        // Tags are seeded; creation/maintenance lives outside Recipe CRUD per SRD §5.2.
        var normalized = tagNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            return new List<RecipeTag>();
        }

        var matches = await db.Tags
            .Where(t => normalized.Contains(t.Name))
            .ToListAsync(ct);

        return matches.Select(t => new RecipeTag { TagId = t.Id, Tag = t }).ToList();
    }

    private static bool TryParseDifficulty(string? value, out Difficulty difficulty)
        => Enum.TryParse(value, ignoreCase: true, out difficulty)
           && Enum.IsDefined(difficulty);

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
