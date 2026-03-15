using InventoryManagement.API.Infrastructure.ExternalServices;

namespace InventoryManagement.API.Features.Products;

// CONTRACT
public record CreateProductRequest(
    string Description,
    decimal AcquisitionCostInSupplierCurrency,
    decimal AcquisitionCostInUsd,
    DateTime AcquireDate,
    string SupplierId,
    string CategoryId
);

public record CreateProductResponse(
    string Id,
    string Description,
    string Status,
    string SupplierId,
    string CategoryId
);


// VALIDATOR
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.AcquisitionCostInSupplierCurrency)
            .GreaterThan(0).WithMessage("Acquisition cost must be positive.");
        RuleFor(x => x.AcquisitionCostInUsd)
            .GreaterThan(0).WithMessage("Acquisition cost in USD must be positive.");
        RuleFor(x => x.AcquireDate)
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Acquire date cannot be in the future.");
    }
}


// USE CASE
public class CreateProductUseCase(
    AppDbContext db, 
    IWmsService wms, 
    IAuditService audit)
{
    // Auth is out of scope — fixed mock
    private const string MockUserId = "system";
    private const string MockUserEmail = "system@inventory.com";

    public async Task<CreateProductResponse> ExecuteAsync(
        CreateProductRequest request, CancellationToken ct)
    {
        // 1. Prerequisite validations
        var supplierExists = await db.Suppliers.AnyAsync(s => s.Id == request.SupplierId, ct);
        if (!supplierExists)
            throw new DomainException($"Supplier '{request.SupplierId}' not found.");

        var category = await db.Categories.FindAsync([request.CategoryId], ct)
            ?? throw new DomainException($"Category '{request.CategoryId}' not found.");

        // 2. Creation and Persistence
        var product = Product.Create(
            request.Description,
            request.AcquisitionCostInSupplierCurrency,
            request.AcquisitionCostInUsd,
            request.AcquireDate,
            request.SupplierId,
            request.CategoryId
        );

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        // 3. Integrations and Log
        await Task.WhenAll(
            wms.CreateProductAsync(product.Id, product.Description, category.Shortcode, product.SupplierId, ct),
            audit.LogAsync(MockUserId, MockUserEmail, AuditEvents.ProductCreated, ct)
        );

        return new CreateProductResponse(
            product.Id,
            product.Description,
            product.Status.ToString(),
            product.SupplierId,
            product.CategoryId
        );
    }
}

// ENDPOINT
public static class CreateProductEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/products", 
            async (
                CreateProductRequest request,
                CreateProductUseCase useCase,
                IValidator<CreateProductRequest> validator,
                CancellationToken ct) 
            => { 
                var validation = await validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await useCase.ExecuteAsync(request, ct);
                return Results.Created($"/products/{result.Id}", result);
            }
        ) 
            .WithName("CreateProduct")
            .WithTags("Products")
            .WithSummary("Create a new product");
    }
}