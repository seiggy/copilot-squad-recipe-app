using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Api.Models;

public class ShareToken
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public Recipe? Recipe { get; set; }
}
