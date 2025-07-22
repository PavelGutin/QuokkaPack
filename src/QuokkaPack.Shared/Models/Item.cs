using QuokkaPack.Data.Models;

namespace QuokkaPack.Shared.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Notes { get; set; }
    public bool IsEssential { get; set; } = false;
    public Guid MasterUserId { get; set; }
    public MasterUser MasterUser { get; set; } = default!;
    public Category Category { get; set; } = default!;
    public int CategoryId { get; set; }
    public ICollection<Trip> Trips { get; set; } = [];
}