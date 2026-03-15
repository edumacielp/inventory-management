using Scalar.AspNetCore;
namespace InventoryManagement.API.Extensions;

public static class ApplicationExtensions
{
    // MIDDLEWARE PIPELINE
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Inventory Management API V1";
                options.Theme = ScalarTheme.Moon;
            });

            // Redirects the root to the Scalar documentation.
            app.MapGet("/", () => Results.Redirect("/scalar/v1"))
               .ExcludeFromDescription();
        }

        app.UseHttpsRedirection();

        return app;
    }

    // DATABASE AUTO-MIGRATION
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Only migrates if there are changes, preventing unnecessary startup delay
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await db.Database.MigrateAsync();
        }
    }
}