namespace QuokkaPack.Shared.DTOs.Category;

public class CategoryEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
