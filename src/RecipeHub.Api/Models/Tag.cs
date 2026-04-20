using System.ComponentModel.DataAnnotations;

namespace RecipeHub.Api.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
}
