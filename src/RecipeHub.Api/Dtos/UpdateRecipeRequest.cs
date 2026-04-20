using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Api.Dtos;

public record UpdateRecipeRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    string Difficulty,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string? ImageUrl,
    string[] Tags,
    RecipeStepDto[] Steps
);
