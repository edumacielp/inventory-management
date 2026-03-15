namespace InventoryManagement.API.Features.Categories;

// CONTRACT
public record CreateCategoryRequest(string Name, string Shortcode, string? ParentCategoryId);
public record CreateCategoryResponse(string Id, string Name, string Shortcode, string? ParentCategoryId);

// VALIDATOR
public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Shortcode).NotEmpty().MaximumLength(20);
    }
}

// USE CASE
public class CreateCategoryUseCase(AppDbContext db)
{
    public async Task<CreateCategoryResponse> ExecuteAsync(
        CreateCategoryRequest request, CancellationToken ct)
    {
        // Validates if the shortcode already exists — uniqueness rule.
        var shortcode = request.Shortcode.Trim().ToUpperInvariant();
        var shortcodeExists = await db.Categories
            .AnyAsync(c => c.Shortcode == shortcode, ct);

        if (shortcodeExists)
            throw new DomainException($"Shortcode '{request.Shortcode}' is already in use.");

        // Validates whether the parent exists, if informed
        if (request.ParentCategoryId is not null)
        {
            var parentExists = await db.Categories
                .AnyAsync(c => c.Id == request.ParentCategoryId, ct);

            if (!parentExists)
                throw new DomainException($"Parent category '{request.ParentCategoryId}' not found.");
        }

        var category = Category.Create(request.Name, request.Shortcode, request.ParentCategoryId);

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return new CreateCategoryResponse(
            category.Id, category.Name, category.Shortcode, category.ParentCategoryId
        );
    }
}

// ENDPOINTS
public static class CreateCategoryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/categories", 
            async (
                CreateCategoryRequest request,
                CreateCategoryUseCase useCase,
                IValidator<CreateCategoryRequest> validator,
                CancellationToken ct) 
            => {
                var validation = await validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await useCase.ExecuteAsync(request, ct);
                return Results.Created($"/categories/{result.Id}", result);
            }
        )
            .WithName("CreateCategory")
            .WithTags("Categories")
            .WithSummary("Create a new category");
    }
}