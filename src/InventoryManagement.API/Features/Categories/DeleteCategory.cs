using InventoryManagement.API.Infrastructure;
using InventoryManagement.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Features.Categories;

// USE CASE
public class DeleteCategoryUseCase(AppDbContext db)
{
    public async Task ExecuteAsync(string id, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct)
            ?? throw new DomainException($"Category '{id}' not found.");

        // Protects referential integrity before deletion.
        var hasChildren = await db.Categories.AnyAsync(c => c.ParentCategoryId == id, ct);
        if (hasChildren)
            throw new DomainException("Cannot delete a category that has subcategories.");

        var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == id, ct);
        if (hasProducts)
            throw new DomainException("Cannot delete a category that has associated products.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}

// ENDPOINT
public static class DeleteCategoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/categories/{id}", 
            async (
                string id,
                DeleteCategoryUseCase useCase,
                CancellationToken ct) 
            => {
                await useCase.ExecuteAsync(id, ct);
                return Results.NoContent();
            }
        )
            .WithName("DeleteCategory")
            .WithTags("Categories")
            .WithSummary("Delete a category");
    }
}