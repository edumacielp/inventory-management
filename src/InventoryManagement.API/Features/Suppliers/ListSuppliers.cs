using InventoryManagement.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Features.Suppliers;

// CONTRACT
public record ListSuppliersResponse(string Id, string Name, string Email, string Currency, string Country);

// USE CASE
public class ListSuppliersUseCase(AppDbContext db)
{
    public async Task<List<ListSuppliersResponse>> ExecuteAsync(CancellationToken ct)
        => await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new ListSuppliersResponse(
                s.Id, s.Name, s.Email, s.Currency, s.Country))
            .ToListAsync(ct);
}

// ENDPOINT
public static class ListSuppliersEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/suppliers", 
            async (
                ListSuppliersUseCase useCase, CancellationToken ct
            ) 
            => Results.Ok(await useCase.ExecuteAsync(ct))
        ) 
            .WithName("ListSuppliers")
            .WithTags("Suppliers")
            .WithSummary("List all suppliers");
    }
}