using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.API.Features.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasMaxLength(36)
            .ValueGeneratedNever();

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.AcquisitionCostInSupplierCurrency)
            .HasPrecision(18, 4);

        builder.Property(p => p.AcquisitionCostInUsd)
            .HasPrecision(18, 4);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.SupplierId)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .HasMaxLength(36)
            .IsRequired();

        builder.HasIndex(p => p.SupplierId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}