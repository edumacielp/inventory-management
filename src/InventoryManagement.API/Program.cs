using InventoryManagement.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES (D.I)
builder.Services
    .AddApiConfiguration()
    .AddInfrastructure(builder.Configuration)
    .AddApplicationUseCases();

var app = builder.Build();

// 2. MIDDLEWARE PIPELINE
app.ConfigurePipeline();

// 3. DATABASE AUTO-MIGRATION
await app.ApplyDatabaseMigrationsAsync();

// 4. ENDPOINTS
app.MapEndpoints();

app.Run();