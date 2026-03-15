using InventoryManagement.API.Shared;

namespace InventoryManagement.API.Features.Categories;

public sealed class Category
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Shortcode { get; private set; } = string.Empty;
    public string? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }

    private Category() { } // EF Core

    public static Category Create(string name, string shortcode, string? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        if (string.IsNullOrWhiteSpace(shortcode))
            throw new DomainException("Category shortcode is required.");

        return new Category
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = name,
            Shortcode = shortcode.ToUpper(),
            ParentCategoryId = parentId
        };
    }
}