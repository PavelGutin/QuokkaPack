namespace QuokkaPack.Shared.DTOs.Category;

/// <summary>
/// Category from the user's catalog
/// </summary>
public class CategoryReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
