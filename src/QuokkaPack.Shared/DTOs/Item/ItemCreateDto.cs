using System.ComponentModel.DataAnnotations;

namespace QuokkaPack.Shared.DTOs.Item
{
    public class ItemCreateDto
    {
        [Required(ErrorMessage = "Item name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Valid category is required")]
        public int CategoryId { get; set; }
    }
}
