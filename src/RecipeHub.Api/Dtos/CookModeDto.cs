namespace RecipeHub.Api.Dtos;

public record CookModeDto(
    int RecipeId,
    int TotalSteps,
    int StepNumber,
    string Instruction,
    int? TimerMinutes
);
