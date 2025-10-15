namespace QuokkaPack.Shared.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsDefault { get; set; } = false;
    public bool IsArchived { get; set; } = false;
    public Guid MasterUserId { get; set; }
    public MasterUser MasterUser { get; set; } = default!;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}