using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.DTOs.ItemDTOs;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Shared.Mappings;

public static class CategoryMappings
{
    public static Category ToCategory(this CategoryCreateDto dto)
    {
        return new Category
        {
            Name = dto.Name,
            IsDefault = dto.IsDefault
        };
    }

    public static void UpdateFromDto(this Category category, CategoryEditDto dto)
    {
        category.Name = dto.Name;
        category.IsDefault = dto.IsDefault;
    }

    public static CategoryReadDto ToReadDto(this Category category)
    {
        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            IsDefault = category.IsDefault//,
            //Items = category.Items.Select(item => item.ToReadDtoSimple())
        };
    }

    // Don't populate items to avoid circular references in some cases
    public static CategoryReadDtoSimple ToReadDtoSimple(this Category category)
    {
        return new CategoryReadDtoSimple
        {
            Id = category.Id,
            Name = category.Name,
            IsDefault = category.IsDefault
        };
    }

    
}
