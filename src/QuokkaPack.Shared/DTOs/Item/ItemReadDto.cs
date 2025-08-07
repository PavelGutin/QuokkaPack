using QuokkaPack.Shared.DTOs.CategoryDTOs;

namespace QuokkaPack.Shared.DTOs.ItemDTOs
{
    public class ItemReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Notes { get; set; }
        public bool IsEssential { get; set; } = false;
        public CategoryReadDtoSimple Category { get; set; } = default!;
    }
}
