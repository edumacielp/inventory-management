using FluentAssertions;
using InventoryManagement.API.Features.Products;
using InventoryManagement.API.Shared;

namespace InventoryManagement.Tests.Products;

public class ProductTests
{
    private static readonly string SupplierId = "supplier-fashionhub";
    private static readonly string CategoryId = "category-jeans";

    private static readonly DateTime Today = DateTime.UtcNow;
    private static readonly DateTime Yesterday = Today.AddDays(-1);

    private static Product CreateValidProduct() 
        => Product.Create(
            "Slim Fit Jeans",
            40m,
            35m,
            Yesterday,
            SupplierId,
            CategoryId
        );

    // CREATE
    [Fact]
    public void Create_ValidData_ShouldReturnProductWithStatusCreated()
    {
        var product = CreateValidProduct();

        product.Id.Should().NotBeEmpty();
        product.Status.Should().Be(ProductStatus.Created);

        product.SoldDate.Should().BeNull();
        product.CancelDate.Should().BeNull();
        product.ReturnDate.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyDescription_ShouldThrowDomainException(string description)
    {
        var act = () 
            => Product.Create(description, 40m, 35m, Yesterday, SupplierId, CategoryId);

        act.Should().Throw<DomainException>().WithMessage("*description*");
    }

    [Fact]
    public void Create_FutureAcquireDate_ShouldThrowDomainException()
    {
        var futureDate = Today.AddDays(1);

        var act = () 
            => Product.Create("Cotton Hoodie", 60m, 50m, futureDate, SupplierId, CategoryId);

        act.Should().Throw<DomainException>().WithMessage("*future*");
    }

    [Fact]
    public void Create_NegativeCost_ShouldThrowDomainException()
    {
        var act = () 
            => Product.Create("Denim Jacket", -10m, 50m, Yesterday, SupplierId, CategoryId);

        act.Should().Throw<DomainException>().WithMessage("*cost*");
    }

    // MARK AS SOLD
    [Fact]
    public void MarkAsSold_FromCreated_ShouldSetStatus()
    {
        var product = CreateValidProduct();

        product.MarkAsSold();

        product.Status.Should().Be(ProductStatus.Sold);
    }

    [Fact]
    public void MarkAsSold_WhenCancelled_ShouldThrowDomainException()
    {
        var product = CreateValidProduct();
        product.Cancel();

        var act = () => product.MarkAsSold();

        act.Should().Throw<DomainException>().WithMessage("*cancelled*");
    }

    [Fact]
    public void MarkAsSold_WhenReturned_ShouldThrowDomainException()
    {
        var product = CreateValidProduct();

        product.MarkAsSold();
        product.Return();

        var act = () => product.MarkAsSold();

        act.Should().Throw<DomainException>().WithMessage("*returned*");
    }

    // CANCEL

    [Fact]
    public void Cancel_FromCreated_ShouldSetStatus()
    {
        var product = CreateValidProduct();

        product.Cancel();

        product.Status.Should().Be(ProductStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrowDomainException()
    {
        var product = CreateValidProduct();
        product.Cancel();

        var act = () => product.Cancel();

        act.Should().Throw<DomainException>().WithMessage("*already cancelled*");
    }

    // RETURN

    [Fact]
    public void Return_FromSold_ShouldSetStatus()
    {
        var product = CreateValidProduct();

        product.MarkAsSold();
        product.Return();

        product.Status.Should().Be(ProductStatus.Returned);
    }
}