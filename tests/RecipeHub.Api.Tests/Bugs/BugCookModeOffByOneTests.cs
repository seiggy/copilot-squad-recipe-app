using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RecipeHub.Api.Dtos;
using Xunit;

namespace RecipeHub.Api.Tests.Bugs;

/// <summary>
/// BUG-001 (Challenge 05): Cook mode off-by-one.
/// <para>
/// <c>src/RecipeHub.Api/Endpoints/CookModeEndpoints.cs</c> ~line 37 subtracts
/// 1 from the incoming <c>stepNumber</c> when querying <c>RecipeSteps</c>:
/// <code>s.StepNumber == stepNumber - 1</code>. Consequently step 1 (the first
/// real step) queries for <c>StepNumber == 0</c> and returns 404, and
/// requesting step N returns step N-1. The last step is unreachable.
/// </para>
/// <para>
/// Fix: remove the <c>- 1</c> subtraction so the endpoint uses the 1-indexed
/// <c>stepNumber</c> directly. Tests in this class describe correct behavior
/// and are <see cref="FactAttribute.Skip"/>-ed until the bug is fixed.
/// </para>
/// </summary>
public class BugCookModeOffByOneTests : IClassFixture<RecipeApiFactory>
{
    private readonly RecipeApiFactory _factory;

    public BugCookModeOffByOneTests(RecipeApiFactory factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "BUG-001: Cook Mode off-by-one - Challenge 05")]
    public async Task GetStep1_ShouldReturnFirstStep_NotSecond()
    {
        // Arrange: the first seeded recipe is "Classic Margherita Pizza" (ID 1)
        // with 6 steps; step 1 begins with "Whisk flour".
        var client = _factory.CreateClient();
        await using var db = _factory.CreateDbContext();
        var firstStep = await db.RecipeSteps
            .AsNoTracking()
            .FirstAsync(s => s.RecipeId == 1 && s.StepNumber == 1);

        // Act
        var response = await client.GetAsync("/api/recipes/1/cook/steps/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<CookModeDto>();
        Assert.NotNull(dto);
        Assert.Equal(1, dto!.StepNumber);
        Assert.Equal(firstStep.Instruction, dto.Instruction);
    }

    [Fact(Skip = "BUG-001: Cook Mode off-by-one - Challenge 05")]
    public async Task GetStep2_ShouldReturnSecondStep()
    {
        var client = _factory.CreateClient();
        await using var db = _factory.CreateDbContext();
        var secondStep = await db.RecipeSteps
            .AsNoTracking()
            .FirstAsync(s => s.RecipeId == 1 && s.StepNumber == 2);

        var response = await client.GetAsync("/api/recipes/1/cook/steps/2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<CookModeDto>();
        Assert.NotNull(dto);
        Assert.Equal(2, dto!.StepNumber);
        Assert.Equal(secondStep.Instruction, dto.Instruction);
    }

    [Fact(Skip = "BUG-001: Cook Mode off-by-one - Challenge 05")]
    public async Task GetStepBeyondTotal_ShouldReturn404()
    {
        var client = _factory.CreateClient();
        await using var db = _factory.CreateDbContext();
        var total = await db.RecipeSteps.CountAsync(s => s.RecipeId == 1);

        var response = await client.GetAsync($"/api/recipes/1/cook/steps/{total + 1}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
