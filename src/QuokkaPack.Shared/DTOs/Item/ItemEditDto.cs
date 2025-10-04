namespace QuokkaPack.Shared.DTOs.Item
{
    public class ItemEditDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
