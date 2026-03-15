using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Shared;

public class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not DomainException domainException)
            return false; // Move on to the next handler (the Global one)

        logger.LogWarning("Domain rule violation: {Message}", domainException.Message);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity, 
            Title = "Business Rule Violation",
            Detail = domainException.Message,
            Instance = $"{context.Request.Method} {context.Request.Path}"
        };

        context.Response.StatusCode = problem.Status.Value;
        await context.Response.WriteAsJsonAsync(problem, ct);

        return true; // Exception successfully handled
    }
}