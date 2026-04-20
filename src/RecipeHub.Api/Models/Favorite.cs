using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Api.Models;

public class Favorite
{
    public int Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    public int RecipeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Recipe? Recipe { get; set; }
}
