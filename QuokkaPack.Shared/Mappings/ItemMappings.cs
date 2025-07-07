using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Shared.Mappings
{
    public static class ItemMappings
    {
        public static Item ToItem(this ItemCreateDto dto)
        {
            return new Item
            {
                Name = dto.Name,
                Notes = dto.Notes,
                IsEssential = dto.IsEssential,
                Categories = dto.Categories
            };
        }

        public static ItemReadDto ToReadDto(this Item item)
        {
            return new ItemReadDto
            {
                Name = item.Name,
                Notes = item.Notes,
                IsEssential = item.IsEssential,
                Categories = item.Categories
            };
        }
    }
}
