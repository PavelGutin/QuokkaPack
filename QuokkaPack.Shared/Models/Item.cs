namespace QuokkaPack.Shared.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Notes { get; set; }
    public bool IsEssential { get; set; } = false;
    public ICollection<Category> Categories { get; set; } = [];
}