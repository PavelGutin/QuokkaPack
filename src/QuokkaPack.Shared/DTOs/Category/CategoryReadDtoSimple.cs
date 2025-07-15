using QuokkaPack.Shared.DTOs.ItemDTOs;

namespace QuokkaPack.Shared.DTOs.CategoryDTOs;

public class CategoryReadDtoSimple
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
}
