using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Dtos;
using RecipeHub.Api.Models;
using Xunit;

namespace RecipeHub.Api.Tests.Bugs;

/// <summary>
/// BUG-002 (Challenge 05): Search is case-sensitive.
/// <para>
/// <c>src/RecipeHub.Api/Endpoints/SearchEndpoints.cs</c> lines 33-34 use
/// <c>string.Contains</c>, which EF Core translates to SQLite's
/// <c>instr()</c> — a case-sensitive operation. Queries like
/// <c>chicken pasta</c> fail to match "Chicken Alfredo Pasta".
/// </para>
/// <para>
/// Fix: switch to case-insensitive matching, e.g.
/// <c>EF.Functions.Like(r.Title, $"%{needle}%")</c> (SQLite <c>LIKE</c> is
/// case-insensitive for ASCII by default) or compare
/// <c>r.Title.ToLower().Contains(needle.ToLower())</c>. Tests describe the
/// correct behavior and are skipped until the fix lands.
/// </para>
/// </summary>
public class BugSearchCaseSensitivityTests : IClassFixture<RecipeApiFactory>
{
    private readonly RecipeApiFactory _factory;

    public BugSearchCaseSensitivityTests(RecipeApiFactory factory)
    {
        _factory = factory;

        // Seed a couple of extra rows with controlled casing. Safe to call
        // repeatedly — guards on title uniqueness.
        using var db = _factory.CreateDbContext();
        SeedControlledRows(db);
    }

    private static void SeedControlledRows(Data.RecipeDbContext db)
    {
        var now = DateTime.UtcNow;

        if (!db.Recipes.Any(r => r.Title == "lowercase lentil stew"))
        {
            db.Recipes.Add(new Recipe
            {
                Title = "lowercase lentil stew",
                Description = "All lowercase title to exercise uppercase queries.",
                Difficulty = Difficulty.Easy,
                PrepTimeMinutes = 5,
                CookTimeMinutes = 30,
                Servings = 4,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        if (!db.Recipes.Any(r => r.Title == "MixedCase Curry Bowl"))
        {
            db.Recipes.Add(new Recipe
            {
                Title = "MixedCase Curry Bowl",
                Description = "Mixed-case title to exercise lowercase queries.",
                Difficulty = Difficulty.Medium,
                PrepTimeMinutes = 10,
                CookTimeMinutes = 25,
                Servings = 2,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        db.SaveChanges();
    }

    [Fact(Skip = "BUG-002: Search case sensitivity - Challenge 05")]
    public async Task Search_WithLowercaseQuery_MatchesMixedCaseTitle()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/recipes/search/?q=mixedcase");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<RecipeDto[]>();
        Assert.NotNull(results);
        Assert.Contains(results!, r => r.Title == "MixedCase Curry Bowl");
    }

    [Fact(Skip = "BUG-002: Search case sensitivity - Challenge 05")]
    public async Task Search_WithUppercaseQuery_MatchesLowercaseTitle()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/recipes/search/?q=LENTIL");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<RecipeDto[]>();
        Assert.NotNull(results);
        Assert.Contains(results!, r => r.Title == "lowercase lentil stew");
    }

    [Fact(Skip = "BUG-002: Search case sensitivity - Challenge 05")]
    public async Task Search_IsCaseInsensitiveAcrossTitleAndDescription()
    {
        var client = _factory.CreateClient();

        // "UPPERCASE" match against the mixed-case description text
        // ("Mixed-case title to exercise lowercase queries.") requires case-
        // insensitive matching against BOTH Title and Description.
        var response = await client.GetAsync("/api/recipes/search/?q=EXERCISE");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<RecipeDto[]>();
        Assert.NotNull(results);
        Assert.Contains(results!, r =>
            r.Title == "MixedCase Curry Bowl" || r.Title == "lowercase lentil stew");
    }
}
