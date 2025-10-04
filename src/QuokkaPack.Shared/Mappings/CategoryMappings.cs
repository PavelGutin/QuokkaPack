using QuokkaPack.Shared.DTOs.Category;
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

    public static CategoryReadDto ToReadDto(this Category category)
    {
        return new CategoryReadDto
        {
            Id = category.Id,
            Name = category.Name,
            IsDefault = category.IsDefault
        };
    }

    public static void UpdateFromDto(this Category category, CategoryEditDto dto)
    {
        category.Name = dto.Name;
        category.IsDefault = dto.IsDefault;
    }
}
