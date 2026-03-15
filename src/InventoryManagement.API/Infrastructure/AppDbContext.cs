using InventoryManagement.API.Features.Categories;
using InventoryManagement.API.Features.Products;
using InventoryManagement.API.Features.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}