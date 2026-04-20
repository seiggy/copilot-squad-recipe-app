using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Dtos;
using Xunit;

namespace RecipeHub.Api.Tests.Bugs;

/// <summary>
/// BUG-003 (Challenge 05): Share token never persisted.
/// <para>
/// <c>src/RecipeHub.Api/Endpoints/ShareEndpoints.cs</c> lines 31-42 create a
/// <c>ShareToken</c> entity, add it to the context, call
/// <c>SaveChangesAsync()</c>, and only THEN set <c>share.Token</c>. Because
/// the entity is no longer being tracked as modified (and no further save
/// occurs), the Token column is stored as an empty string. The API returns a
/// freshly-generated token in the response body that has no corresponding row
/// in the DB, so <c>GET /api/shared/{token}</c> always returns 404.
/// </para>
/// <para>
/// Fix: assign <c>share.Token = Guid.NewGuid().ToString("N")</c> BEFORE
/// <c>db.ShareTokens.Add(share)</c>, or add a second <c>SaveChangesAsync</c>
/// after assigning. Tests describe correct behavior and are skipped until
/// the fix lands.
/// </para>
/// </summary>
public class BugSharePersistenceTests : IClassFixture<RecipeApiFactory>
{
    private readonly RecipeApiFactory _factory;

    public BugSharePersistenceTests(RecipeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "BUG-003: Share token persistence - Challenge 05")]
    public async Task PostShare_ShouldPersistTokenToDatabase()
    {
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/recipes/1/share", content: null);

        // Assert response body
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ShareDto>();
        Assert.NotNull(dto);
        Assert.False(string.IsNullOrWhiteSpace(dto!.Token));

        // Assert the token landed in the DB with matching value.
        await using var db = _factory.CreateDbContext();
        var saved = await db.ShareTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.RecipeId == 1 && t.Token == dto.Token);

        Assert.NotNull(saved);
        Assert.False(string.IsNullOrWhiteSpace(saved!.Token));
        Assert.Equal(dto.Token, saved.Token);
    }

    [Fact(Skip = "BUG-003: Share token persistence - Challenge 05")]
    public async Task GetSharedToken_ShouldReturnRecipe()
    {
        var client = _factory.CreateClient();

        var postResponse = await client.PostAsync("/api/recipes/1/share", content: null);
        postResponse.EnsureSuccessStatusCode();
        var share = await postResponse.Content.ReadFromJsonAsync<ShareDto>();
        Assert.NotNull(share);

        var getResponse = await client.GetAsync($"/api/shared/{share!.Token}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var recipe = await getResponse.Content.ReadFromJsonAsync<RecipeDetailDto>();
        Assert.NotNull(recipe);
        Assert.Equal(1, recipe!.Id);
    }
}
