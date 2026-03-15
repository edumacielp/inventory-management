using FluentAssertions;
using InventoryManagement.API.Features.Categories;
using InventoryManagement.API.Features.Products;
using InventoryManagement.API.Features.Suppliers;
using InventoryManagement.API.Infrastructure;
using InventoryManagement.API.Infrastructure.ExternalServices;
using InventoryManagement.API.Shared;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace InventoryManagement.Tests.Products;

public class ChangeProductStatusUseCaseTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IWmsService _wms;
    private readonly IAuditService _audit;
    private readonly IEmailService _email;
    private readonly ChangeProductStatusUseCase _sut;

    public ChangeProductStatusUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.CreateVersion7().ToString())
            .Options;

        _db = new AppDbContext(options);

        _wms = Substitute.For<IWmsService>();
        _audit = Substitute.For<IAuditService>();
        _email = Substitute.For<IEmailService>();

        _sut = new ChangeProductStatusUseCase(_db, _wms, _audit, _email);
    }

    // HELPERS
    private async Task<(Product product, Supplier supplier)> SeedProductAsync(
        ProductStatus status = ProductStatus.Created)
    {
        var supplier = Supplier.Create(
            "UrbanWear",
            "supply@urbanwear.com",
            "USD",
            "US"
        );

        var category = Category.Create("Hoodies", "HDY");

        _db.Suppliers.Add(supplier);
        _db.Categories.Add(category);

        var product = Product.Create(
            "Cotton Hoodie",
            35m,
            30m,
            DateTime.UtcNow.AddDays(-1),
            supplier.Id,
            category.Id);

        if (status == ProductStatus.Sold)
            product.MarkAsSold();

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return (product, supplier);
    }

    // SELL
    [Fact]
    public async Task ExecuteAsync_Sell_ShouldUpdateStatusAndTriggerSideEffects()
    {
        var (product, supplier) = await SeedProductAsync();

        var request = new ChangeProductStatusRequest(
            product.Id,
            ProductStatus.Sold
        );

        var response = await _sut.ExecuteAsync(product.Id, request, CancellationToken.None);

        response.Status.Should().Be("Sold");
        response.SoldDate.Should().NotBeNull();

        await _email.Received(1)
            .SendSupplierNotificationAsync(
                supplier.Email,
                product.Id,
                Arg.Any<CancellationToken>()
            );

        await _wms.Received(1)
            .DispatchProductAsync(
                product.Id,
                Arg.Any<CancellationToken>()
            );
    }

    // CANCEL
    [Fact]
    public async Task ExecuteAsync_Cancel_ShouldUpdateStatusWithoutExternalCalls()
    {
        var (product, _) = await SeedProductAsync();

        var request = new ChangeProductStatusRequest(
            product.Id,
            ProductStatus.Cancelled
        );

        var response = await _sut.ExecuteAsync(product.Id, request, CancellationToken.None);

        response.Status.Should().Be("Cancelled");
        response.CancelDate.Should().NotBeNull();

        await _email.DidNotReceive()
            .SendSupplierNotificationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await _wms.DidNotReceive()
            .DispatchProductAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );
    }

    // RETURN
    [Fact]
    public async Task ExecuteAsync_Return_ShouldUpdateStatus()
    {
        var (product, _) = await SeedProductAsync(ProductStatus.Sold);

        var request = new ChangeProductStatusRequest(
            product.Id,
            ProductStatus.Returned
        );

        var response = await _sut.ExecuteAsync(product.Id, request, CancellationToken.None);

        response.Status.Should().Be("Returned");
        response.ReturnDate.Should().NotBeNull();

        await _email.DidNotReceive()
            .SendSupplierNotificationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}