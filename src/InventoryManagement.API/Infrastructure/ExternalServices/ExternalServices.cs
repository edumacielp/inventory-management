namespace InventoryManagement.API.Infrastructure.ExternalServices;

// WMS
public sealed class WmsService(ILogger<WmsService> logger) : IWmsService
{
    public Task CreateProductAsync(
        string productId, string description, string categoryShortcode, string supplierId,
        CancellationToken ct = default)
    {
        logger.LogWmsCreate(productId, description, categoryShortcode, supplierId);
        return Task.CompletedTask;
    }

    public Task DispatchProductAsync(string productId, CancellationToken ct = default)
    {
        logger.LogWmsDispatch(productId);
        return Task.CompletedTask;
    }
}

// AUDIT
public sealed class AuditService(ILogger<AuditService> logger) : IAuditService
{
    public Task LogAsync(
        string userId, string email, string actionName,
        CancellationToken ct = default)
    {
        logger.LogAudit(userId, email, actionName, DateTime.UtcNow);
        return Task.CompletedTask;
    }
}

// EMAIL
public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendSupplierNotificationAsync(
        string supplierEmail, string productId,
        CancellationToken ct = default)
    {
        logger.LogEmail(supplierEmail, productId);
        return Task.CompletedTask;
    }
}

// LOG EXTENSIONS (source generator)
// Zero allocations in the hot path — the compiler generates the implementation
// at build time, without string interpolation at runtime.
public static partial class ExternalServiceLogs
{
    [LoggerMessage(Level = LogLevel.Information,
        Message = "[WMS MOCK] POST /products → productId: {ProductId}, description: {Description}, category: {CategoryShortcode}, supplierId: {SupplierId}")]
    public static partial void LogWmsCreate(this ILogger logger, string productId, string description, string categoryShortcode, string supplierId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "[WMS MOCK] POST /products/{ProductId}/dispatch")]
    public static partial void LogWmsDispatch(this ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "[AUDIT MOCK] POST /logs → userId: {UserId}, email: {Email}, action: {ActionName}, timestamp: {Timestamp}")]
    public static partial void LogAudit(this ILogger logger, string userId, string email, string actionName, DateTime timestamp);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "[EMAIL MOCK] Sending supplier notification → email: {SupplierEmail}, productId: {ProductId}")]
    public static partial void LogEmail(this ILogger logger, string supplierEmail, string productId);
}