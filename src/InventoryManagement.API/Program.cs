using InventoryManagement.API.Api.Exceptions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES (D.I)
builder.Services.AddOpenApi();

// Global Error Handling & RFC 7807 Standard
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// 2. MIDDLEWARE PIPELINE
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Inventory Management API V1";
        options.Theme = ScalarTheme.Moon;
    });

    app.MapGet("/", () => Results.Redirect("/scalar/v1"))
       .ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.Run();