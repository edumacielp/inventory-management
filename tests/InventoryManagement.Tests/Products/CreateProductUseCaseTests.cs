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

public class CreateProductUseCaseTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IWmsService _wms;
    private readonly IAuditService _audit;
    private readonly CreateProductUseCase _sut;

    public CreateProductUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _wms = Substitute.For<IWmsService>();
        _audit = Substitute.For<IAuditService>();

        _sut = new CreateProductUseCase(_db, _wms, _audit);
    }

    // HELPERS

    private async Task<(string supplierId, string categoryId)> SeedPrerequisitesAsync()
    {
        var supplier = Supplier.Create(
            "FashionHub",
            "contact@fashionhub.com",
            "USD",
            "US"
        );

        var category = Category.Create("Jeans", "JNS");

        _db.Suppliers.Add(supplier);
        _db.Categories.Add(category);

        await _db.SaveChangesAsync();

        return (supplier.Id, category.Id);
    }

    private static CreateProductRequest ValidRequest(string supplierId, string categoryId)
        => new(
            Description: "Slim Fit Jeans",
            AcquisitionCostInSupplierCurrency: 40m,
            AcquisitionCostInUsd: 35m,
            AcquireDate: DateTime.UtcNow.AddDays(-1),
            SupplierId: supplierId,
            CategoryId: categoryId
        );

    // CREATE
    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldPersistProduct()
    {
        var (supplierId, categoryId) = await SeedPrerequisitesAsync();

        var request = ValidRequest(supplierId, categoryId);

        var response = await _sut.ExecuteAsync(request, CancellationToken.None);

        var saved = await _db.Products.FindAsync(response.Id);

        saved.Should().NotBeNull();
        saved!.Description.Should().Be("Slim Fit Jeans");
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldCallExternalServices()
    {
        var (supplierId, categoryId) = await SeedPrerequisitesAsync();

        var request = ValidRequest(supplierId, categoryId);

        await _sut.ExecuteAsync(request, CancellationToken.None);

        await _wms.Received(1)
            .CreateProductAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await _audit.Received(1)
            .LogAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                "PRODUCT_CREATED",
                Arg.Any<CancellationToken>()
            );
    }

    // VALIDATIONS
    [Fact]
    public async Task ExecuteAsync_SupplierNotFound_ShouldThrowDomainException()
    {
        var (_, categoryId) = await SeedPrerequisitesAsync();

        var request = ValidRequest("invalid-supplier", categoryId);

        var act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("*Supplier*");
    }

    [Fact]
    public async Task ExecuteAsync_CategoryNotFound_ShouldThrowDomainException()
    {
        var (supplierId, _) = await SeedPrerequisitesAsync();

        var request = ValidRequest(supplierId, "invalid-category");

        var act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        await act.Should()
            .ThrowAsync<DomainException>()
            .WithMessage("*Category*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPrerequisitesMissing_ShouldNotCallExternalServices()
    {
        var request = ValidRequest("invalid-supplier", "invalid-category");

        try
        {
            await _sut.ExecuteAsync(request, CancellationToken.None);
        }
        catch { }

        await _wms.DidNotReceive()
            .CreateProductAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()
            );

        await _audit.DidNotReceive()
            .LogAsync(
                Arg.Any<string>(),
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