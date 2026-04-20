namespace RecipeHub.Api.Dtos;

public record RecipeStepDto(
    int StepNumber,
    string Instruction,
    int? TimerMinutes
);
