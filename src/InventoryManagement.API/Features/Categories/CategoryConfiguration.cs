using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.API.Features.Categories;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasMaxLength(36)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Shortcode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.ParentCategoryId)
            .HasMaxLength(36);

        builder.HasIndex(c => c.Shortcode)
            .IsUnique();

        builder.HasOne(c => c.ParentCategory)
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}