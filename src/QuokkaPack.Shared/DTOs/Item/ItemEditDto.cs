using QuokkaPack.Shared.Models;

namespace QuokkaPack.Shared.DTOs.ItemDTOs
{
    public class ItemEditDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Notes { get; set; }
        public bool IsEssential { get; set; } = false;
        public Category Category { get; set; } = default!;
    }
}
