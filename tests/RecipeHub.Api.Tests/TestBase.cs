namespace RecipeHub.Api.Tests;

/// <summary>
/// Shared base for integration tests that need a WebApplicationFactory-backed
/// HttpClient and a SQLite in-memory database with a shared open connection.
///
/// Intentionally empty — the concrete fixture, DI overrides, transaction
/// scoping, and seed helpers land in Item 21 (first real integration tests)
/// per the RecipeHub test strategy (§2.1 Backend testing infrastructure).
/// </summary>
public abstract class TestBase
{
}
