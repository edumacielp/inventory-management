namespace InventoryManagement.API.Infrastructure.ExternalServices;

public interface IWmsService
{
    Task CreateProductAsync(string productId, string description, string categoryShortcode, string supplierId, CancellationToken ct = default);
    Task DispatchProductAsync(string productId, CancellationToken ct = default);
}

public interface IAuditService
{
    Task LogAsync(string userId, string email, string actionName, CancellationToken ct = default);
}

public interface IEmailService
{
    Task SendSupplierNotificationAsync(string supplierEmail, string productId, CancellationToken ct = default);
}