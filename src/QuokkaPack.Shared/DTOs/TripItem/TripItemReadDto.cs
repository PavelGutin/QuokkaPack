namespace QuokkaPack.Shared.DTOs.TripItem
{
    /// <summary>
    /// Represents an item that is IN a trip (denormalized for convenience)
    /// </summary>
    public class TripItemReadDto
    {
        public int TripItemId { get; set; }        // TripItem.Id
        public int ItemId { get; set; }            // Item.Id (for client-side join)
        public string ItemName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsPacked { get; set; }
    }
}
