using InventoryManagement.API.Infrastructure.ExternalServices;

namespace InventoryManagement.API.Features.Products;


// CONTRACT
public record ChangeProductStatusRequest(
    string ProductId,
    ProductStatus NewStatus
);

public record ChangeProductStatusResponse(
    string Id,
    string Status,
    DateTime? SoldDate,
    DateTime? CancelDate,
    DateTime? ReturnDate
);


// VALIDATOR
public class ChangeProductStatusValidator : AbstractValidator<ChangeProductStatusRequest>
{
    private static readonly ProductStatus[] Allowed =
    [
        ProductStatus.Sold,
        ProductStatus.Cancelled,
        ProductStatus.Returned
    ];

    public ChangeProductStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .Must(status => Allowed.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", Allowed)}.");
    }
}


// USE CASE
public class ChangeProductStatusUseCase(
    AppDbContext db,
    IWmsService wms,
    IAuditService audit,
    IEmailService email)
{
    private const string MockUserId = "system";
    private const string MockUserEmail = "system@inventory.com";

    public async Task<ChangeProductStatusResponse> ExecuteAsync(
        string productId, ChangeProductStatusRequest request, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == productId, ct)
                ?? throw new DomainException($"Product '{productId}' not found.");

        // 1. Executes the state transition in the domain.
        var auditEvent = ApplyStatusChange(product, request.NewStatus);

        // 2. Persistence
        await db.SaveChangesAsync(ct);

        // 3. Side Effects (Orchestration)
        await Task.WhenAll(
           NotifyExternalSystemsAsync(product, ct),
           audit.LogAsync(MockUserId, MockUserEmail, auditEvent, ct)
        );

        return new ChangeProductStatusResponse(
            product.Id,
            product.Status.ToString(),
            product.SoldDate,
            product.CancelDate,
            product.ReturnDate
        );
    }

    private static string ApplyStatusChange(Product product, ProductStatus status)
    {
        switch (status)
        {
            case ProductStatus.Sold:
                product.MarkAsSold();
                return AuditEvents.ProductSold;

            case ProductStatus.Cancelled:
                product.Cancel();
                return AuditEvents.ProductCancelled;

            case ProductStatus.Returned:
                product.Return();
                return AuditEvents.ProductReturned;

            default:
                throw new DomainException($"Cannot manually set status to '{status}'.");
        }
    }

    private async Task NotifyExternalSystemsAsync(Product product, CancellationToken ct)
    {
        if (product.Status != ProductStatus.Sold) return;

        await email.SendSupplierNotificationAsync(product.Supplier.Email, product.Id, ct);
        await wms.DispatchProductAsync(product.Id, ct);
    }
}


// ENDPOINT
public static class ChangeProductStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPatch(
            "/products/{id}/status", 
            async (
                string id,
                ChangeProductStatusRequest request,
                ChangeProductStatusUseCase useCase,
                IValidator<ChangeProductStatusRequest> validator,
                CancellationToken ct) 
            => {
                var validation = await validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await useCase.ExecuteAsync(id, request, ct);
                return Results.Ok(result);
            }
        )
            .WithName("ChangeProductStatus")
            .WithTags("Products")
            .WithSummary("Change product status (Sold / Cancelled / Returned)");
    }
}