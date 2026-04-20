using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Api.Models;

public class RecipeStep
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int StepNumber { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Instruction { get; set; } = string.Empty;

    public int? TimerMinutes { get; set; }

    public Recipe? Recipe { get; set; }
}
