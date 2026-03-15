using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.API.Features.Suppliers;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasMaxLength(36)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Email)
            .IsUnique();
    }
}