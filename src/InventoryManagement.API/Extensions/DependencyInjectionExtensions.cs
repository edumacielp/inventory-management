using InventoryManagement.API.Features.Categories;
using InventoryManagement.API.Features.Products;
using InventoryManagement.API.Features.Suppliers;
using InventoryManagement.API.Infrastructure.ExternalServices;
namespace InventoryManagement.API.Extensions;

public static class DependencyInjectionExtensions
{
    // VALIDATIONS & DOCS
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        // API Docs
        services.AddOpenApi();

        // Global Error Handling & RFC 7807 Standard
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }

    // DATABASE & EXTERNAL SERVICES
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IWmsService, WmsService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    // USE CASES
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateCategoryUseCase>();
        services.AddScoped<ListCategoriesUseCase>();
        services.AddScoped<DeleteCategoryUseCase>();

        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<ChangeProductStatusUseCase>();

        services.AddScoped<CreateSupplierUseCase>();
        services.AddScoped<ListSuppliersUseCase>();

        return services;
    }
}