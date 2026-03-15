using FluentAssertions;
using InventoryManagement.API.Features.Categories;
using InventoryManagement.API.Shared;

namespace InventoryManagement.Tests.Categories;

public class CategoryTests
{
    [Fact]
    public void Create_ValidData_ShouldReturnCategory()
    {
        var category = Category.Create("Shirts", "shi");

        category.Id.Should().NotBeEmpty();
        category.Name.Should().Be("Shirts");
        category.Shortcode.Should().Be("SHI");
        category.ParentCategoryId.Should().BeNull();
    }

    [Fact]
    public void Create_WithParent_ShouldSetParentId()
    {
        var parentId = "category-men";

        var category = Category.Create("T-Shirts", "tee", parentId);

        category.ParentCategoryId.Should().Be(parentId);
    }

    [Theory]
    [InlineData("", "SHI")]
    [InlineData("   ", "SHI")]
    [InlineData("Shirts", "")]
    [InlineData("Shirts", "   ")]
    public void Create_InvalidNameOrShortcode_ShouldThrowDomainException(string name, string shortcode)
    {
        var act = () => Category.Create(name, shortcode);

        act.Should().Throw<DomainException>();
    }
}