# RecipeHub Test Strategy

📌 **Proactive:** drafted while Mal was producing the overall build plan. Will need a pass once Mal's plan lands.

**Author:** Zoe (Tester / QA)  
**Date:** 2026-04-20  
**Status:** Draft

---

## 1. Scope

### 1.1 What We Test (v1)

- **API Endpoints:** Full coverage of RecipeEndpoints, TagEndpoints, CookModeEndpoints, SearchEndpoints, ShareEndpoints. Stub behavior of FavoriteEndpoints (501 responses).
- **Business Logic:** Recipe CRUD validation, step sequencing, tag associations, search filtering, cook mode navigation logic, share token generation and expiry.
- **Data Layer:** EF Core entity relationships, cascade deletes, seed data integrity, SQLite date handling.
- **Frontend Components:** RecipeCard, RecipeForm, CookModePage, SearchBar, FilterPanel — behavior only, not visual regression.
- **API Integration:** Frontend API client error handling, TanStack Query cache behavior, optimistic updates (when implemented).
- **Planted Bugs:** Three SKIPPED tests representing expected-after-fix behavior for Challenge 05.

### 1.2 What We Don't Test (v1)

- **Deployment:** No Azure/cloud infrastructure tests. App is local-only.
- **Authentication/Authorization:** None exists. No need to test.
- **Performance:** No load testing, no latency SLAs for a hackathon demo.
- **Visual Regression:** No screenshot diffing, no pixel-perfect layout validation.
- **Accessibility:** Would be nice but not blocking for v1 (defer to post-hackathon improvements).
- **Browser Compatibility:** React 19 modern browser baseline only. No IE11, no mobile-specific testing.
- **Network Failure Simulation:** Offline mode, retries, circuit breakers — out of scope for v1.

---

## 2. Test Projects / Tooling

### 2.1 Backend — `tests/RecipeHub.Api.Tests`

**Framework:** xUnit 2.9+

**Testing Infrastructure:**
- `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory)
- **Database: SQLite in-memory with shared connection** (recommended)
  - **Rationale:** SQLite in-memory with a shared open connection mimics real SQLite file behavior (including foreign key constraints, triggers if added, transaction semantics) while allowing test isolation through transaction rollback. EF Core InMemory provider lacks constraint enforcement and is not suitable for testing real EF queries.
  - **Pattern:** Open a single `SqliteConnection` with `"Data Source=:memory:"`, keep it open for the test lifetime, inject it into WebApplicationFactory's DbContext, wrap each test in a transaction, rollback after assertion.
  
**Assertions:** FluentAssertions for readability

**HTTP Client:** Typed HttpClient from WebApplicationFactory

**Test Isolation:** Each test class gets a fresh database schema via migrations, seeded data applied once per class, individual tests use scoped transactions that roll back.

### 2.2 Frontend — `tests/RecipeHub.Web.Tests`

**Framework:** Vitest 2.x + React Testing Library

**Mocking Strategy:**
- MSW (Mock Service Worker) for API responses
- `vi.mock()` for module-level fakes (e.g., `navigator.wakeLock`)

**Component Testing:**
- User-event simulation (`@testing-library/user-event`)
- Render with `QueryClientProvider` wrapper for TanStack Query components
- Test IDs via `data-testid` for critical navigation elements only

**No DOM Access Required:** Tests run in jsdom environment (no headless browser).

### 2.3 E2E — Playwright (Optional / Deferred)

**Recommendation:** **Defer to post-v1.**

**Rationale:**
- Integration tests via WebApplicationFactory already cover full request/response cycles.
- Frontend RTL tests cover user interactions within components.
- Playwright adds value for flows that span multiple pages (create recipe → cook mode → share link), but those can be tested with integration + component tests in v1.
- Challenge 05 bug diagnosis doesn't require E2E — the bugs are visible in API responses and component state.

**If included later:**
- Codegen for happy-path flows (recipe creation, search, cook mode)
- Use Playwright's locators (avoid brittle CSS selectors)
- One smoke test per major flow

---

## 3. Test Pyramid

### 3.1 Target Distribution

| Layer | Count (Approx) | Purpose |
|-------|----------------|---------|
| **Unit** (C# domain logic) | ~15 | Entity validation, DTO mapping, enum parsing |
| **Integration** (API) | ~40 | Endpoint behavior, DB queries, error responses |
| **Component** (React) | ~25 | UI interaction, form validation, timer behavior |
| **E2E** (Playwright) | 0 (v1) | Deferred |

**Total:** ~80 tests

### 3.2 Rationale

- **Heavy on integration:** RecipeHub is data-driven. Most value comes from testing the API → EF Core → SQLite stack end-to-end.
- **Moderate component testing:** Focus on stateful components (CookModePage, RecipeForm) and complex UI (search filters). Simple presentational components (RecipeCard, Badge) are low priority.
- **Minimal pure unit tests:** .NET Minimal API has little extractable business logic. Most validation lives in EF/data annotations.

---

## 4. Acceptance Criteria Map

### 4.1 Recipe CRUD (SRD §5.1)

| Test Case | Type | Project |
|-----------|------|---------|
| GET_AllRecipes_Returns200WithRecipeList | Integration | Api.Tests |
| GET_RecipeById_ValidId_ReturnsRecipeDetailDto | Integration | Api.Tests |
| GET_RecipeById_InvalidId_Returns404 | Integration | Api.Tests |
| POST_CreateRecipe_ValidData_Returns201WithLocation | Integration | Api.Tests |
| POST_CreateRecipe_MissingTitle_Returns400 | Integration | Api.Tests |
| POST_CreateRecipe_TitleExceeds200Chars_Returns400 | Integration | Api.Tests |
| POST_CreateRecipe_WithSteps_StepsAreSequential | Integration | Api.Tests |
| PUT_UpdateRecipe_ValidData_Returns200AndUpdatesTimestamp | Integration | Api.Tests |
| PUT_UpdateRecipe_NonExistentId_Returns404 | Integration | Api.Tests |
| DELETE_Recipe_ValidId_Returns204AndCascadesStepsAndTags | Integration | Api.Tests |
| DELETE_Recipe_NonExistentId_Returns404 | Integration | Api.Tests |

### 4.2 Tags and Filtering (SRD §5.2)

| Test Case | Type | Project |
|-----------|------|---------|
| GET_Tags_ReturnsAllSeedTags | Integration | Api.Tests |
| GET_RecipesByTag_SingleTag_ReturnsMatchingRecipes | Integration | Api.Tests |
| GET_RecipesByTag_MultipleTags_ReturnsIntersection | Integration | Api.Tests |
| GET_RecipesByDifficulty_Easy_ReturnsOnlyEasyRecipes | Integration | Api.Tests |
| GET_RecipesByMaxTime_30Minutes_ReturnsRecipesUnder30 | Integration | Api.Tests |

### 4.3 Cook Mode (SRD §5.3, BUG 1)

| Test Case | Type | Project | Notes |
|-----------|------|---------|-------|
| GET_CookModeSteps_ValidRecipeId_ReturnsAllStepsInOrder | Integration | Api.Tests | |
| GET_CookModeStep_StepNumber1_ReturnsFirstStep | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 1 |
| GET_CookModeStep_LastStepNumber_ReturnsLastStep | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 1 |
| CookModePage_LoadsWithStep1Displayed | Component | Web.Tests | **SKIPPED** — fails pre-fix due to Bug 1 |
| CookModePage_NextButton_AdvancesToStep2 | Component | Web.Tests | |
| CookModePage_PreviousButton_OnStep1_Disabled | Component | Web.Tests | **SKIPPED** — fails pre-fix due to Bug 1 |
| CookModePage_StepWithTimer_DisplaysCountdownButton | Component | Web.Tests | |
| CookModePage_ReachLastStep_NextButtonDisabled | Component | Web.Tests | **SKIPPED** — fails pre-fix due to Bug 1 |

### 4.4 Search (SRD §5.4, BUG 2)

| Test Case | Type | Project | Notes |
|-----------|------|---------|-------|
| GET_Search_SingleWordLowercase_ReturnsCaseInsensitiveResults | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 2 |
| GET_Search_MultiWordQuery_ReturnsMatchingRecipes | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 2 |
| GET_Search_QueryInDescription_ReturnsRecipe | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 2 |
| GET_Search_ExactCaseMatch_ReturnsResults | Integration | Api.Tests | Works pre-fix |
| GET_Search_WithTagFilter_CombinesFilters | Integration | Api.Tests | |
| SearchBar_TypesQuery_TriggersAPICall | Component | Web.Tests | |

### 4.5 Share (SRD §5.5, BUG 3)

| Test Case | Type | Project | Notes |
|-----------|------|---------|-------|
| POST_ShareRecipe_ValidId_ReturnsShareDto | Integration | Api.Tests | Returns 200 even with bug |
| POST_ShareRecipe_TokenIsPersisted_InDatabase | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 3 |
| GET_SharedRecipe_ValidToken_ReturnsRecipeDetailDto | Integration | Api.Tests | **SKIPPED** — fails pre-fix due to Bug 3 |
| GET_SharedRecipe_ExpiredToken_Returns404 | Integration | Api.Tests | |
| ShareButton_Click_DisplaysShareURL | Component | Web.Tests | |
| SharedRecipePage_ValidToken_RendersRecipe | Component | Web.Tests | **SKIPPED** — fails pre-fix due to Bug 3 |

### 4.6 Favorites (SRD §5.6, Stub Only)

| Test Case | Type | Project | Notes |
|-----------|------|---------|-------|
| GET_Favorites_ReturnsNotImplemented | Integration | Api.Tests | Validates 501 response |
| POST_Favorites_ReturnsNotImplemented | Integration | Api.Tests | Validates 501 response |
| DELETE_Favorites_ReturnsNotImplemented | Integration | Api.Tests | Validates 501 response |
| FavoritesPage_DisplaysComingSoonMessage | Component | Web.Tests | |

### 4.7 Data Quality (data-assessment.md §5)

| Test Case | Type | Project |
|-----------|------|---------|
| SeedData_AllRecipes_HaveTitleCase | Integration | Api.Tests |
| SeedData_AllRecipeSteps_AreSequentialFrom1 | Integration | Api.Tests |
| SeedData_StepsWithoutTimer_HaveNullTimerMinutes | Integration | Api.Tests |
| SeedData_Tags_SeededBeforeRecipes | Integration | Api.Tests |
| SeedData_ShareTokens_IsEmpty | Integration | Api.Tests |
| CreateRecipe_CascadeDelete_RemovesStepsAndTags | Integration | Api.Tests |

---

## 5. Planted Bug Test Specs

### 5.1 Bug 1: Cook Mode Off-By-One

**Test Name (Backend):** `GET_CookModeStep_StepNumber1_ReturnsFirstStep`

**Location:** `tests/RecipeHub.Api.Tests/CookModeEndpointsTests.cs`

**Skip Marker:**
```csharp
[Fact(Skip = "Bug 1: Cook Mode off-by-one. Endpoint subtracts 1 from stepNumber, causing step 1 to look for StepNumber == 0.")]
public async Task GET_CookModeStep_StepNumber1_ReturnsFirstStep()
{
    // Arrange: Recipe with 6 steps (Margherita Pizza, ID 1)
    var client = _factory.CreateClient();
    
    // Act: Request step 1
    var response = await client.GetAsync("/api/recipes/1/cook/steps/1");
    
    // Assert: Should return 200 with StepNumber == 1
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var step = await response.Content.ReadFromJsonAsync<CookStepDto>();
    step.Should().NotBeNull();
    step!.StepNumber.Should().Be(1);
    step.Instruction.Should().Contain("Preheat"); // First step of Margherita Pizza
}
```

**Test Name (Frontend):** `CookModePage_LoadsWithStep1Displayed`

**Location:** `tests/RecipeHub.Web.Tests/CookModePage.test.tsx`

**Skip Marker:**
```typescript
it.skip('loads with step 1 displayed', () => {
  // Bug 1: Cook Mode starts at index 1 instead of 0, skipping first step
  const steps = [
    { stepNumber: 1, instruction: 'Preheat oven', timerMinutes: null },
    { stepNumber: 2, instruction: 'Mix ingredients', timerMinutes: 5 },
  ];
  
  render(<CookModePage recipeId={1} steps={steps} />);
  
  expect(screen.getByText(/step 1 of 2/i)).toBeInTheDocument();
  expect(screen.getByText(/preheat oven/i)).toBeInTheDocument();
});
```

**Expected Assertion (after fix):**
- Backend: `GET /api/recipes/{id}/cook/steps/1` returns the step where `StepNumber == 1`, not 0.
- Frontend: `useCookMode` hook initializes `currentStepIndex` to 0, displaying `steps[0]` as "Step 1 of N".

---

### 5.2 Bug 2: Search Case Sensitivity

**Test Name:** `GET_Search_MultiWordLowercaseQuery_ReturnsCaseInsensitiveResults`

**Location:** `tests/RecipeHub.Api.Tests/SearchEndpointsTests.cs`

**Skip Marker:**
```csharp
[Fact(Skip = "Bug 2: Search uses string.Contains (case-sensitive). Query 'chicken pasta' won't match 'Chicken Alfredo Pasta'.")]
public async Task GET_Search_MultiWordLowercaseQuery_ReturnsCaseInsensitiveResults()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act: Search "chicken pasta" (lowercase)
    var response = await client.GetAsync("/api/search?q=chicken%20pasta");
    
    // Assert: Should match "Chicken Alfredo Pasta" (ID 3)
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var results = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();
    results.Should().NotBeEmpty();
    results.Should().Contain(r => r.Id == 3); // Chicken Alfredo Pasta
}
```

**Test Name (Description Search):** `GET_Search_QueryInDescription_ReturnsRecipe`

**Location:** `tests/RecipeHub.Api.Tests/SearchEndpointsTests.cs`

**Skip Marker:**
```csharp
[Fact(Skip = "Bug 2: Search only searches Title, not Description. Expected behavior is to search both.")]
public async Task GET_Search_QueryInDescription_ReturnsRecipe()
{
    // Arrange: Assume seed data includes recipe with "basil" only in description
    var client = _factory.CreateClient();
    
    // Act: Search for "basil"
    var response = await client.GetAsync("/api/search?q=basil");
    
    // Assert: Should return recipes where description contains "basil"
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var results = await response.Content.ReadFromJsonAsync<List<RecipeDto>>();
    results.Should().NotBeEmpty("search should match Description field");
}
```

**Expected Assertion (after fix):**
- Replace `r.Title.Contains(word)` with `EF.Functions.Like(r.Title, $"%{word}%") || EF.Functions.Like(r.Description ?? "", $"%{word}%")` for case-insensitive, description-inclusive search.

---

### 5.3 Bug 3: Share Token Persistence

**Test Name:** `POST_ShareRecipe_TokenIsPersisted_InDatabase`

**Location:** `tests/RecipeHub.Api.Tests/ShareEndpointsTests.cs`

**Skip Marker:**
```csharp
[Fact(Skip = "Bug 3: Share token generated AFTER SaveChangesAsync, so it's never persisted. DB row has Token = null.")]
public async Task POST_ShareRecipe_TokenIsPersisted_InDatabase()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act: Create share token for recipe 1
    var response = await client.PostAsync("/api/recipes/1/share", null);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var shareDto = await response.Content.ReadFromJsonAsync<ShareDto>();
    shareDto.Should().NotBeNull();
    shareDto!.Token.Should().NotBeNullOrEmpty();
    
    // Assert: Token should exist in database
    // Direct DB query (requires injecting DbContext or using separate connection)
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
    var savedToken = await db.ShareTokens.FirstOrDefaultAsync(t => t.Token == shareDto.Token);
    
    savedToken.Should().NotBeNull("token should be persisted in ShareTokens table");
    savedToken!.RecipeId.Should().Be(1);
}
```

**Test Name (Frontend):** `SharedRecipePage_ValidToken_RendersRecipe`

**Location:** `tests/RecipeHub.Web.Tests/SharedRecipePage.test.tsx`

**Skip Marker:**
```typescript
it.skip('renders recipe when valid token is provided', async () => {
  // Bug 3: Token never persisted, so GET /api/shared/{token} always returns 404
  server.use(
    http.get('/api/shared/valid-token-123', () => {
      return HttpResponse.json({
        id: 1,
        title: 'Margherita Pizza',
        steps: [{ stepNumber: 1, instruction: 'Preheat oven', timerMinutes: null }],
      });
    })
  );
  
  render(<SharedRecipePage token="valid-token-123" />);
  
  await waitFor(() => {
    expect(screen.getByText(/margherita pizza/i)).toBeInTheDocument();
  });
});
```

**Expected Assertion (after fix):**
- Move `share.Token = Guid.NewGuid().ToString("N");` BEFORE `db.ShareTokens.Add(share);` so the complete entity is saved.
- Verify `GET /api/shared/{token}` returns 200 with RecipeDetailDto.

---

## 6. Data Strategy

### 6.1 Backend Test Database Setup

**Per-Class Fixture:**
```csharp
public class RecipeEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private SqliteConnection _connection;
    
    public RecipeEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace scoped DbContext with in-memory SQLite
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RecipeDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                services.AddDbContext<RecipeDbContext>(options =>
                    options.UseSqlite(_connection));
            });
        });
    }
    
    public async Task InitializeAsync()
    {
        // Apply migrations and seed data once per test class
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
        await db.Database.EnsureCreatedAsync();
        // Seed data is applied automatically via DbContext configuration
    }
    
    public async Task DisposeAsync()
    {
        _connection.Close();
        await _connection.DisposeAsync();
    }
}
```

**Per-Test Isolation:** Wrap each test in a transaction (optional, depends on test interdependence). Simpler approach: reset DB before each test if mutations conflict.

### 6.2 Frontend Test Data

**MSW Handlers:**
- Define reusable mock responses in `tests/mocks/handlers.ts`
- Use seed data structure (12 recipes, 10 tags) for consistency with backend
- Override per-test with `server.use()` for edge cases (404, validation errors)

**Example:**
```typescript
export const handlers = [
  http.get('/api/recipes', () => {
    return HttpResponse.json(seedRecipes); // Array of RecipeDto
  }),
  
  http.get('/api/recipes/:id', ({ params }) => {
    const recipe = seedRecipes.find(r => r.id === Number(params.id));
    return recipe 
      ? HttpResponse.json(recipe)
      : new HttpResponse(null, { status: 404 });
  }),
];
```

### 6.3 Test Pollution Avoidance

- **Backend:** In-memory SQLite connection is isolated per test class. If tests modify data, either:
  - Use read-only assertions (test seed data integrity)
  - Reset DB via `db.Database.EnsureDeletedAsync(); db.Database.EnsureCreatedAsync();` in test cleanup
- **Frontend:** MSW server resets handlers after each test automatically (`afterEach(() => server.resetHandlers())`).

### 6.4 Seed Data Reuse

- Backend tests assume seed data is present (12 recipes, 10 tags, 0 share tokens).
- Any test that creates new entities should use unique identifiers to avoid ID collisions.
- Document which seed entities are used in test comments (e.g., "Recipe ID 1 = Margherita Pizza with 6 steps").

---

## 7. CI Considerations

### 7.1 Required for Pull Request Merge

- ✅ All backend integration tests pass
- ✅ All frontend component tests pass
- ✅ Test coverage ≥ 70% (lines)
- ✅ No skipped tests except the 3 planted bug tests

### 7.2 Pre-Merge Checks (GitHub Actions)

```yaml
name: Test Suite
on: [pull_request]
jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet test tests/RecipeHub.Api.Tests --logger "trx;LogFileName=test-results.trx"
      - name: Upload Test Results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: backend-test-results
          path: '**/test-results.trx'
  
  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm ci
        working-directory: client
      - run: npm test -- --run --coverage
        working-directory: client
```

### 7.3 Coverage Thresholds

Configured in `vitest.config.ts`:
```typescript
export default defineConfig({
  test: {
    coverage: {
      lines: 70,
      functions: 70,
      branches: 65,
      statements: 70,
    },
  },
});
```

### 7.4 Manual Testing Gates

- **Smoke Test:** After deployment (local), manually verify:
  - Recipe list page loads with seed data
  - Creating a new recipe succeeds
  - Cook mode opens (will skip step 1 pre-fix, expected)
- **Bug Verification:** Before marking Challenge 05 complete, manually:
  - Test share link (should 404 pre-fix, work post-fix)
  - Search "chicken pasta" (0 results pre-fix, 1+ post-fix)
  - Cook mode step 1 (skipped pre-fix, visible post-fix)

---

## 8. Open Questions

### 8.1 For Mal (Build Plan Owner)

- **Test execution order:** Should planted bug tests be in a separate test suite that runs conditionally (e.g., `dotnet test --filter "FullyQualifiedName!~PlantedBugs"`)? Or is skipping via `[Fact(Skip = "...")]` sufficient?
- **Database reset strategy:** Do we want a CLI command (e.g., `dotnet run --reset-db`) for coaches to demonstrate the seed data reset flow, or is "delete recipes.db" acceptable?

### 8.2 For Zack (Product Owner)

- **Accessibility testing:** Should we add `axe-core` (via `vitest-axe`) to catch basic a11y violations (e.g., missing alt text, unlabeled buttons)? Low effort, high teaching value.
- **E2E Priority:** If we defer Playwright for v1, do we need a smoke test checklist doc for coaches to run manually before the hackathon?
- **Bug Fix Acceptance:** After students fix a bug, should they:
  - Un-skip the test and verify it passes?
  - Write a NEW test that validates the fix (leaving the skipped test as historical record)?
  - Recommend: **Un-skip the test.** The test describes the correct behavior; once the bug is fixed, the test should pass.

### 8.3 Technical Ambiguities (SRD)

- **Cook Mode Timer Persistence:** If a user starts a 12-minute timer, navigates away from Cook Mode, then returns — should the timer state be lost or persisted? SRD §6.3 mentions `navigator.wakeLock` but doesn't specify localStorage or in-memory state management.
  - **Assumption for testing:** Timer state is lost on navigation. Test only that timer starts correctly within a single session.
  
- **Search Query Normalization:** Should the fix use `ToLower()` (C# normalization) or `EF.Functions.Like()` (SQL LIKE, which is case-insensitive in SQLite by default)? 
  - **Recommendation:** `EF.Functions.Like()` — translates to `LIKE` operator, no overhead of lowercasing strings in C#.

- **Share Token Uniqueness:** Is it acceptable for two shares of the same recipe to generate two distinct tokens, or should subsequent shares return the existing token if it hasn't expired?
  - **SRD implication (§8.3):** "Sharing the same recipe twice creates two distinct tokens" — test for this.

---

**Next Steps:**
1. Review with Mal once build plan is published.
2. Confirm database reset strategy (CLI vs manual file deletion).
3. Get Zack's guidance on accessibility testing scope.
4. Implement test projects (backend first, frontend concurrent).
5. Write seed data integrity tests before feature tests to establish baseline.
