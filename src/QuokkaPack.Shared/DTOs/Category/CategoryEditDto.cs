﻿namespace QuokkaPack.Shared.DTOs.CategoryDTOs;

public class CategoryEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
}
