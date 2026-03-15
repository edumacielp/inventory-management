using InventoryManagement.API.Features.Categories;
using InventoryManagement.API.Features.Products;
using InventoryManagement.API.Features.Suppliers;
namespace InventoryManagement.API.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        // Category
        CreateCategoryEndpoint.Map(app);
        ListCategoriesEndpoint.Map(app);
        DeleteCategoryEndpoint.Map(app);

        // Products
        CreateProductEndpoint.Map(app);
        ChangeProductStatusEndpoint.Map(app);

        // Supplier
        CreateSupplierEndpoint.Map(app);
        ListSuppliersEndpoint.Map(app);

        return app;
    }
}