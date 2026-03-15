using FluentValidation;
using InventoryManagement.API.Infrastructure;
using InventoryManagement.API.Shared;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Features.Suppliers;

// CONTRACT
public record CreateSupplierRequest(string Name, string Email, string Currency, string Country);
public record CreateSupplierResponse(string Id, string Name, string Email, string Currency, string Country);

// VALIDATOR
public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Currency).NotEmpty().Length(10);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

// USE CASE
public class CreateSupplierUseCase(AppDbContext db)
{
    public async Task<CreateSupplierResponse> ExecuteAsync(
        CreateSupplierRequest request, CancellationToken ct)
    {
        var emailExists = await db.Suppliers
            .AnyAsync(s => s.Email.Equals(request.Email, StringComparison.InvariantCultureIgnoreCase), ct);

        if (emailExists)
            throw new DomainException($"A supplier with email '{request.Email}' already exists.");

        var supplier = Supplier.Create(request.Name, request.Email, request.Currency, request.Country);

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(ct);

        return new CreateSupplierResponse(
            supplier.Id, supplier.Name, supplier.Email, supplier.Currency, supplier.Country
       );
    }
}

// ENDPOINT
public static class CreateSupplierEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/suppliers", 
            async (
                CreateSupplierRequest request,
                CreateSupplierUseCase useCase,
                IValidator<CreateSupplierRequest> validator,
                CancellationToken ct) 
            => {
                var validation = await validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await useCase.ExecuteAsync(request, ct);
                return Results.Created($"/suppliers/{result.Id}", result);
            }
        )
            .WithName("CreateSupplier")
            .WithTags("Suppliers")
            .WithSummary("Create a new supplier");
    }
}