using QuokkaPack.Data.Models;

namespace QuokkaPack.Shared.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public Guid MasterUserId { get; set; }
    public MasterUser MasterUser { get; set; } = default!;
    public ICollection<Trip> Trips { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
}