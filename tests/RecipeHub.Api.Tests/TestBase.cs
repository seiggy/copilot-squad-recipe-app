using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeHub.Api.Data;

namespace RecipeHub.Api.Tests;

/// <summary>
/// Shared WebApplicationFactory for integration tests. Uses a unique
/// temp-file SQLite database per factory instance so every test class gets
/// an isolated DB with migrations applied and <see cref="SeedData"/> populated
/// (Program.cs runs both at startup). The file is deleted on disposal.
///
/// Kept intentionally small — this is a teaching codebase. Test classes
/// consume this factory via <c>IClassFixture&lt;RecipeApiFactory&gt;</c>.
/// </summary>
public sealed class RecipeApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"recipehub-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // Point the Api at a throwaway SQLite file; Program.cs will Migrate +
        // Seed against it at startup.
        builder.UseSetting("ConnectionStrings:RecipeDb", $"Data Source={_dbPath}");
    }

    /// <summary>
    /// Opens a fresh scope and returns the configured <see cref="RecipeDbContext"/>.
    /// Tests use this to read/write the DB directly (e.g. seeding extra rows or
    /// asserting persistence).
    /// </summary>
    public RecipeDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            TryDeleteDb();
        }
    }

    private void TryDeleteDb()
    {
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch
        {
            // Best-effort cleanup. Leaked temp files are harmless.
        }
    }
}

