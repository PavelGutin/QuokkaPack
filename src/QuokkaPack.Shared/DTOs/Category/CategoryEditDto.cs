using System.ComponentModel.DataAnnotations;

namespace QuokkaPack.Shared.DTOs.Category;

public class CategoryEditDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
