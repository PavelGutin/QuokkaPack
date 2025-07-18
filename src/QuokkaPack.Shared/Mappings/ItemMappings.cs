﻿using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.DTOs.Trip;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Shared.Mappings
{
    public static class ItemMappings
    {
        public static Item ToItem(this ItemCreateDto dto)
        {
            throw new NotImplementedException("Fix this category mapping");
            return new Item
            {
                Name = dto.Name,
                Notes = dto.Notes,
                IsEssential = dto.IsEssential//,
                //Categories = dto.Categories
            };
        }

        public static ItemReadDto ToReadDto(this Item item)
        {
            return new ItemReadDto
            {
                Id = item.Id,
                Name = item.Name,
                Notes = item.Notes,
                IsEssential = item.IsEssential,
                Categories = item.Categories.Select(category => category.ToReadDtoSimple()).ToList()
            };
        }

        public static ItemReadDtoSimple ToReadDtoSimple(this Item item)
        {
            return new ItemReadDtoSimple
            {
                Id = item.Id,
                Name = item.Name,
                Notes = item.Notes,
                IsEssential = item.IsEssential
            };
        }

        
    }
}
