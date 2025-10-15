namespace QuokkaPack.Shared.DTOs.Item
{
    /// <summary>
    /// Item from the user's catalog (denormalized for client convenience)
    /// </summary>
    public class ItemReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
    }
}
