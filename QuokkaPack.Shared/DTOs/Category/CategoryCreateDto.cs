namespace QuokkaPack.Shared.DTOs.Category;

public class CategoryCreateDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
}
