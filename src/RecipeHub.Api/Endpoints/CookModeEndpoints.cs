using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Data;
using RecipeHub.Api.Dtos;

namespace RecipeHub.Api.Endpoints;

public static class CookModeEndpoints
{
    public static void MapCookModeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes/{id:int}/cook").WithTags("CookMode");

        group.MapGet("/steps/{stepNumber:int}", GetStepAsync);
    }

    private static async Task<IResult> GetStepAsync(
        int id,
        int stepNumber,
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

        var totalSteps = await db.RecipeSteps
            .Where(s => s.RecipeId == id)
            .CountAsync(ct);

        var step = await db.RecipeSteps
            .AsNoTracking()
            .Where(s => s.RecipeId == id && s.StepNumber == stepNumber - 1)
            .FirstOrDefaultAsync(ct);

        if (step is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new CookModeDto(
            id,
            totalSteps,
            stepNumber,
            step.Instruction,
            step.TimerMinutes
        ));
    }
}
