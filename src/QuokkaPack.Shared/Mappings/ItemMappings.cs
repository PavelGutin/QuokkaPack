using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.DTOs.Trip;
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
                CategoryId = dto.CategoryId
            };
        }

        public static ItemReadDto ToReadDto(this Item item)
        {
            return new ItemReadDto
            {
                Id = item.Id,
                Name = item.Name,
                Category = item?.Category?.ToReadDtoSimple()
            };
        }

        public static ItemReadDtoSimple ToReadDtoSimple(this Item item)
        {
            return new ItemReadDtoSimple
            {
                Id = item.Id,
                Name = item.Name
            };
        }

        
    }
}
