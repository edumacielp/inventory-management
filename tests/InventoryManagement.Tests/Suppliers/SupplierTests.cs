using FluentAssertions;
using InventoryManagement.API.Features.Suppliers;
using InventoryManagement.API.Shared;

namespace InventoryManagement.Tests.Suppliers;

public class SupplierTests
{
    [Fact]
    public void Create_ValidData_ShouldReturnSupplier()
    {
        var supplier = Supplier.Create("FashionHub", "contact@fashionhub.com", "usd", "US");

        supplier.Id.Should().NotBeEmpty();
        supplier.Name.Should().Be("FashionHub");
        supplier.Email.Should().Be("contact@fashionhub.com");
        supplier.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData("", "contact@fashionhub.com", "USD", "US")]
    [InlineData("FashionHub", "", "USD", "US")]
    [InlineData("FashionHub", "contact@fashionhub.com", "", "US")]
    [InlineData("FashionHub", "contact@fashionhub.com", "USD", "")]
    public void Create_MissingRequiredField_ShouldThrowDomainException(
        string name, string email, string currency, string country)
    {
        var act = () => Supplier.Create(name, email, currency, country);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_InvalidEmail_ShouldThrowDomainException()
    {
        var act = () => Supplier.Create("FashionHub", "invalid-email", "USD", "US");

        act.Should().Throw<DomainException>().WithMessage("*email*");
    }
}