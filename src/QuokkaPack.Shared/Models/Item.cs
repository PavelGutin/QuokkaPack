namespace QuokkaPack.Shared.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid MasterUserId { get; set; }
    public MasterUser MasterUser { get; set; } = default!;
    public Category Category { get; set; } = default!;
    public int CategoryId { get; set; }
    public ICollection<TripItem> TripItems { get; set; } = new List<TripItem>();
}