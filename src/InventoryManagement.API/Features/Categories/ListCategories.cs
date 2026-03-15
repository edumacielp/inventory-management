using InventoryManagement.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Features.Categories;

// CONTRACT
public record ListCategoriesResponse(
    string Id,
    string Name,
    string Shortcode,
    string? ParentCategoryId,
    string? ParentCategoryName);

// USE CASE
public class ListCategoriesUseCase(AppDbContext db)
{
    public async Task<List<ListCategoriesResponse>> ExecuteAsync(CancellationToken ct)
        => await db.Categories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .Select(c => new ListCategoriesResponse(
                c.Id,
                c.Name,
                c.Shortcode,
                c.ParentCategoryId,
                c.ParentCategory != null
                    ? c.ParentCategory.Name
                    : null
            ))
            .ToListAsync(ct);
}

// ENDPOINTS
public static class ListCategoriesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/categories", 
            async (
                ListCategoriesUseCase useCase, CancellationToken ct
            ) 
            => Results.Ok(await useCase.ExecuteAsync(ct))
        ) 
            .WithName("ListCategories")
            .WithTags("Categories")
            .WithSummary("List all categories");
    }
}