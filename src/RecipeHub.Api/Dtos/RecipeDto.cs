namespace RecipeHub.Api.Dtos;

public record RecipeDto(
    int Id,
    string Title,
    string? Description,
    string Difficulty,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string? ImageUrl,
    string[] TagNames
);
