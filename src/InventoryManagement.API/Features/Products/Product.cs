using InventoryManagement.API.Features.Categories;
using InventoryManagement.API.Features.Suppliers;
using InventoryManagement.API.Shared;

namespace InventoryManagement.API.Features.Products;

public sealed class Product
{
    public string Id { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal AcquisitionCostInSupplierCurrency { get; private set; }
    public decimal AcquisitionCostInUsd { get; private set; }
    public ProductStatus Status { get; private set; }

    public DateTime AcquireDate { get; private set; }
    public DateTime? SoldDate { get; private set; }
    public DateTime? CancelDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }

    public string SupplierId { get; private set; } = string.Empty;
    public Supplier Supplier { get; private set; } = default!;

    public string CategoryId { get; private set; } = string.Empty;
    public Category Category { get; private set; } = default!;

    private Product() { } // EF Core

    public static Product Create(
        string description,
        decimal acquisitionCostInSupplierCurrency,
        decimal acquisitionCostInUsd,
        DateTime acquireDate,
        string supplierId,
        string categoryId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Product description is required.");

        if (string.IsNullOrWhiteSpace(supplierId))
            throw new DomainException("Supplier is required.");

        if (string.IsNullOrWhiteSpace(categoryId))
            throw new DomainException("Category is required.");

        if (acquisitionCostInSupplierCurrency <= 0)
            throw new DomainException("Acquisition cost in supplier currency must be positive.");

        if (acquisitionCostInUsd <= 0)
            throw new DomainException("Acquisition cost in USD must be positive.");

        if (acquireDate > DateTime.UtcNow)
            throw new DomainException("Acquire date cannot be in the future.");

        return new Product
        {
            Id = Guid.CreateVersion7().ToString(),
            Description = description.Trim(),
            AcquisitionCostInSupplierCurrency = acquisitionCostInSupplierCurrency,
            AcquisitionCostInUsd = acquisitionCostInUsd,
            AcquireDate = acquireDate,
            Status = ProductStatus.Created,
            SupplierId = supplierId,
            CategoryId = categoryId
        };
    }

    public void MarkAsSold()
    {
        if (Status == ProductStatus.Cancelled)
            throw new DomainException("Cancelled products cannot be sold.");

        if (Status == ProductStatus.Returned)
            throw new DomainException("Returned products cannot be sold.");

        if (Status == ProductStatus.Sold)
            throw new DomainException("Product is already sold.");

        Status = ProductStatus.Sold;
        SoldDate = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ProductStatus.Cancelled)
            throw new DomainException("Product is already cancelled.");

        Status = ProductStatus.Cancelled;
        CancelDate = DateTime.UtcNow;
    }

    public void Return()
    {
        if (Status == ProductStatus.Returned)
            throw new DomainException("Product is already returned.");

        Status = ProductStatus.Returned;
        ReturnDate = DateTime.UtcNow;
    }
}