using QuokkaPack.Shared.DTOs.CategoryDTOs;
using QuokkaPack.Shared.Models;

namespace QuokkaPack.Shared.Mappings;

public static class CategoryMappings
{
    public static Category ToCategory(this CategoryCreateDto dto)
    {
        return new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IsDefault = dto.IsDefault
        };
    }

    public static void UpdateFromDto(this Category category, CategoryEditDto dto)
    {
        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsDefault = dto.IsDefault;
    }

    public static CategoryReadDto ToReadDto(this Category category)
    {
        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsDefault = category.IsDefault
        };
    }
}
