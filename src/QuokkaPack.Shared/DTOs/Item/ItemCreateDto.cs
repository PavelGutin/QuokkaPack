namespace QuokkaPack.Shared.DTOs.ItemDTOs
{
    public class ItemCreateDto
    {
        public string Name { get; set; } = default!;
        public string? Notes { get; set; }
        public bool IsEssential { get; set; } = false;
        public int CategoryId {get; set;}
    }
}
