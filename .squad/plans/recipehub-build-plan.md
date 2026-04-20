# RecipeHub Build Plan

**Author:** Mal (Lead / Architect)  
**Date:** 2026-04-20  
**Directive:** Build RecipeHub using .NET 10, EF Core + SQLite, React 19 + Vite 6, and Aspire 13.2 as base template

---

## 1. Summary

RecipeHub is a full-stack recipe management app built as the target codebase for the GitHub Copilot & Squad Developer Workflow Hackathon. We're scaffolding on **.NET Aspire 13.2** with a .NET 10 Minimal API backend, SQLite via EF Core 10, and a React 19 + TypeScript + Vite 6 frontend orchestrated as a ViteApp resource. The project ships with **three intentional bugs** in secondary features (Cook Mode off-by-one, Search case sensitivity, Share token persistence) — these are Challenge 05 material and **must not be fixed**. Every architectural choice prioritizes teachability and clarity over cleverness. This is a hackathon starter; participants need to understand the code in under 20 minutes.

---

## 2. Repo Structure

```
RecipeHub.sln
src/
  RecipeHub.AppHost/              # Aspire 13.2 AppHost (.NET 10)
    Program.cs                    # Orchestrates Api + Web + SQLite config
    RecipeHub.AppHost.csproj
  RecipeHub.ServiceDefaults/      # Aspire ServiceDefaults (.NET 10)
    Extensions.cs                 # OpenTelemetry, health checks, resilience
    RecipeHub.ServiceDefaults.csproj
  RecipeHub.Api/                  # .NET 10 Minimal API + EF Core 10 + SQLite
    Program.cs
    appsettings.json
    Data/
      RecipeDbContext.cs
      SeedData.cs
    Models/
      Recipe.cs, RecipeStep.cs, Tag.cs, RecipeTag.cs
      Favorite.cs, ShareToken.cs, Difficulty.cs
    Endpoints/
      RecipeEndpoints.cs          # CRUD — working
      TagEndpoints.cs             # working
      CookModeEndpoints.cs        # 🐛 BUG 1 — DO NOT FIX
      SearchEndpoints.cs          # 🐛 BUG 2 — DO NOT FIX
      ShareEndpoints.cs           # 🐛 BUG 3 — DO NOT FIX
      FavoriteEndpoints.cs        # stubs (501) — Ch02 builds this
    Dtos/
      RecipeDto.cs, RecipeDetailDto.cs, CookModeDto.cs
      CreateRecipeRequest.cs, ShareDto.cs
    RecipeHub.Api.csproj
  RecipeHub.Web/                  # React 19 + TypeScript + Vite 6
    package.json
    vite.config.ts
    tsconfig.json
    index.html
    src/
      main.tsx
      App.tsx
      api/client.ts
      components/
        ui/                       # Button, Card, Spinner, Badge, Timer
        recipe/                   # RecipeCard, RecipeList, RecipeForm
        cook-mode/                # CookModeView, StepDisplay, StepTimer
        search/                   # SearchBar, FilterPanel
      hooks/
        useRecipes.ts
        useCookMode.ts            # 🐛 BUG 1 — DO NOT FIX
        useTimer.ts
        useSearch.ts
      pages/
        HomePage.tsx
        RecipeListPage.tsx
        RecipeDetailPage.tsx
        RecipeEditPage.tsx
        CookModePage.tsx
        SharedRecipePage.tsx
        FavoritesPage.tsx         # shell — Ch02 builds this
      types/recipe.ts
tests/
  RecipeHub.Api.Tests/            # xUnit + WebApplicationFactory
    RecipeHub.Api.Tests.csproj
    (no test files — Ch04)
  RecipeHub.Web.Tests/            # Vitest + RTL (config only)
    vitest.config.ts
    (no test files — Ch04)
.devcontainer/
  devcontainer.json
  Dockerfile
.editorconfig
.gitignore
README.md
```

---

## 3. Aspire AppHost Design

### 3.1 Package References

Per Aspire 13.2 docs and `aspire-list_integrations`:

| Package | Purpose |
|---------|---------|
| `Aspire.Hosting.JavaScript` (13.2.2) | `AddViteApp()` for React frontend |
| `CommunityToolkit.Aspire.Hosting.Sqlite` (13.2.1-beta) | SQLite resource (optional — see 3.3) |

### 3.2 AppHost Program.cs

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// SQLite is file-based — no Aspire resource needed for local dev
// Connection string passed as parameter or config

var api = builder.AddProject<Projects.RecipeHub_Api>("api")
    .WithExternalHttpEndpoints();

// Vite app with Aspire 13.2 JavaScript integration
// AddViteApp auto-registers http endpoint + PORT env var
var web = builder.AddViteApp("web", "../RecipeHub.Web")
    .WithReference(api)
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"));

builder.Build().Run();
```

**Key decisions (cite: Aspire docs `javascript-integration`):**

1. **`AddViteApp()`** — Do NOT call `.WithHttpEndpoint()` on Vite resources; it auto-registers one and sets `PORT`. Calling it again causes duplicate endpoint errors.

2. **API URL Discovery** — Per Aspire docs, Vite only exposes `VITE_`-prefixed env vars to client code. We pass `VITE_API_BASE_URL` via `WithEnvironment()`. The React app reads `import.meta.env.VITE_API_BASE_URL`.

3. **SQLite** — File-based, lives at `src/RecipeHub.Api/recipes.db`. No Aspire SQLite resource needed for local dev. The Community Toolkit `CommunityToolkit.Aspire.Hosting.Sqlite` exists but adds complexity for no benefit in this scenario. We configure connection string in `appsettings.json` and inject via ServiceDefaults.

### 3.3 ServiceDefaults

Standard Aspire pattern:

```csharp
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
        return builder;
    }
}
```

The Api project calls `builder.AddServiceDefaults()` in `Program.cs`.

---

## 4. Backend Plan (Kaylee)

### 4.1 EF Core Model Shape

Per SRD §4 and Data Assessment §2:

| Entity | Key Properties | Relationships |
|--------|---------------|---------------|
| Recipe | Id, Title, Description, PrepTimeMinutes, CookTimeMinutes, Servings, Difficulty (enum), ImageUrl, CreatedAt, UpdatedAt | Has many Steps, RecipeTags, ShareTokens, Favorites |
| RecipeStep | Id, RecipeId, StepNumber (1-indexed), Instruction, TimerMinutes? | Belongs to Recipe |
| Tag | Id, Name (unique) | Has many RecipeTags |
| RecipeTag | RecipeId, TagId (composite PK) | Junction |
| ShareToken | Id, RecipeId, Token (unique), CreatedAt, ExpiresAt | Belongs to Recipe |
| Favorite | Id, UserId, RecipeId, CreatedAt | Belongs to Recipe |

### 4.2 API Routes

| Method | Route | Status | Notes |
|--------|-------|--------|-------|
| GET | /api/recipes | ✅ | Paginated list |
| GET | /api/recipes/{id} | ✅ | Detail with steps/tags |
| POST | /api/recipes | ✅ | Create |
| PUT | /api/recipes/{id} | ✅ | Update |
| DELETE | /api/recipes/{id} | ✅ | Delete |
| GET | /api/tags | ✅ | All tags |
| GET | /api/tags/{id}/recipes | ✅ | Recipes by tag |
| GET | /api/recipes/{id}/cook | ✅ | Cook mode data |
| GET | /api/recipes/{id}/cook/steps/{stepNumber} | 🐛 BUG 1 | Off-by-one |
| GET | /api/recipes/search | 🐛 BUG 2 | Case-sensitive |
| POST | /api/recipes/{id}/share | 🐛 BUG 3 | Token not persisted |
| GET | /api/shared/{token} | 🐛 BUG 3 | Always 404 |
| GET | /api/favorites | 501 | Stub — Ch02 |
| POST | /api/favorites | 501 | Stub — Ch02 |
| DELETE | /api/favorites/{recipeId} | 501 | Stub — Ch02 |

### 4.3 Migration Strategy

- Initial migration `InitialCreate` with all 6 tables
- `UseAsyncSeeding` (EF 9+ pattern) for 12 recipes, 10 tags, ~69 steps
- Connection string: `Data Source=recipes.db` (file in Api project root)
- Reset: delete `recipes.db` and restart

### 4.4 Planted Bug Locations — DO NOT FIX (Challenge 05)

| Bug | File | Root Cause |
|-----|------|------------|
| **BUG 1: Cook Mode Off-By-One** | `CookModeEndpoints.cs` line ~15 | `stepNumber - 1` applied to 1-indexed data |
| **BUG 1 (frontend)** | `useCookMode.ts` line ~3 | `useState(1)` should be `useState(0)` |
| **BUG 2: Search Case Sensitivity** | `SearchEndpoints.cs` line ~18 | `string.Contains()` is case-sensitive in SQLite |
| **BUG 3: Share Token Persistence** | `ShareEndpoints.cs` line ~12 | Token assigned after `SaveChangesAsync()` |

---

## 5. Frontend Plan (Inara)

### 5.1 Route Map

| Route | Page | Notes |
|-------|------|-------|
| `/` | HomePage | Hero, featured recipes |
| `/recipes` | RecipeListPage | Grid, search, filters |
| `/recipes/:id` | RecipeDetailPage | Full view, Cook Mode button, Share |
| `/recipes/new` | RecipeEditPage | Create mode |
| `/recipes/:id/edit` | RecipeEditPage | Edit mode |
| `/recipes/:id/cook` | CookModePage | Step-by-step with timers |
| `/shared/:token` | SharedRecipePage | Read-only view |
| `/favorites` | FavoritesPage | "Coming Soon" shell |

### 5.2 Component Hierarchy

```
App
├── Navigation
├── Routes
│   ├── HomePage → HeroSection, FeaturedRecipes → RecipeCard[]
│   ├── RecipeListPage → SearchBar, FilterPanel, RecipeCard[], Pagination
│   ├── RecipeDetailPage → RecipeHeader, StepList, TagList, CookModeButton, ShareButton
│   ├── CookModePage → ProgressBar, StepDisplay, StepTimer, StepNavigation
│   ├── RecipeEditPage → RecipeForm (controlled, dynamic steps)
│   ├── SharedRecipePage → RecipeDetailView (read-only)
│   └── FavoritesPage → "Coming Soon"
└── Footer
```

### 5.3 TanStack Query Key Strategy

```typescript
// Query keys for cache invalidation
const recipeKeys = {
  all: ['recipes'] as const,
  lists: () => [...recipeKeys.all, 'list'] as const,
  list: (page: number, filters: Filters) => [...recipeKeys.lists(), { page, ...filters }] as const,
  details: () => [...recipeKeys.all, 'detail'] as const,
  detail: (id: number) => [...recipeKeys.details(), id] as const,
  cook: (id: number) => [...recipeKeys.detail(id), 'cook'] as const,
};

const tagKeys = {
  all: ['tags'] as const,
};

const shareKeys = {
  byToken: (token: string) => ['shared', token] as const,
};
```

### 5.4 API URL Discovery

Aspire injects `VITE_API_BASE_URL` at runtime:

```typescript
// src/api/client.ts
const API_BASE = import.meta.env.VITE_API_BASE_URL ?? '';

export async function fetchRecipes(page: number): Promise<PaginatedResult<Recipe>> {
  const res = await fetch(`${API_BASE}/api/recipes?page=${page}`);
  if (!res.ok) throw new Error('Failed to fetch');
  return res.json();
}
```

For local dev without Aspire, Vite proxy config in `vite.config.ts` handles `/api/*` → backend.

---

## 6. Test Plan (Zoe)

### 6.1 Test Projects

| Project | Framework | Coverage |
|---------|-----------|----------|
| `RecipeHub.Api.Tests` | xUnit + WebApplicationFactory | Unit + integration |
| `RecipeHub.Web.Tests` | Vitest + React Testing Library | Component + hook |

### 6.2 Coverage Strategy

**Backend (xUnit):**
- Recipe CRUD endpoints (happy path, 404s, validation)
- Tag filtering
- Cook Mode step retrieval
- Search (multi-word, filters)
- Share create/retrieve

**Frontend (Vitest):**
- RecipeCard rendering
- RecipeForm validation
- useCookMode hook (step navigation)
- useSearch hook (debounce)
- API client error handling

### 6.3 Planted Bug Tests — SKIPPED

Each planted bug gets a test file with skipped tests that participants un-skip in Challenge 05:

**Backend (`RecipeHub.Api.Tests/BugTests/`):**

```csharp
public class CookModeOffByOneTests
{
    [Fact(Skip = "BUG-001: Cook Mode off-by-one - Challenge 05")]
    public async Task GetStep1_ReturnsFirstStep_NotSecond()
    {
        // Arrange, Act, Assert for step 1 returning correct content
    }

    [Fact(Skip = "BUG-001: Cook Mode off-by-one - Challenge 05")]
    public async Task GetLastStep_ReturnsLastStep_Not404()
    {
        // Test last step is reachable
    }
}

public class SearchCaseSensitivityTests
{
    [Fact(Skip = "BUG-002: Search case sensitivity - Challenge 05")]
    public async Task Search_LowercaseChicken_FindsChickenAlfredo() { }

    [Fact(Skip = "BUG-002: Search case sensitivity - Challenge 05")]
    public async Task Search_MultiWord_ChickenPasta_FindsResults() { }
}

public class ShareTokenPersistenceTests
{
    [Fact(Skip = "BUG-003: Share token persistence - Challenge 05")]
    public async Task CreateShareToken_PersistsToDatabase() { }

    [Fact(Skip = "BUG-003: Share token persistence - Challenge 05")]
    public async Task GetSharedRecipe_ByToken_ReturnsRecipe() { }
}
```

**Frontend (`RecipeHub.Web.Tests/bugs/`):**

```typescript
describe.skip('BUG-001: Cook Mode off-by-one - Challenge 05', () => {
  it('starts at step 1, not step 2', () => { });
  it('can navigate to the last step', () => { });
});
```

---

## 7. Work Decomposition

| # | Title | Owner | Depends | Parallel with |
|---|-------|-------|---------|---------------|
| 1 | Scaffold Aspire solution (AppHost + ServiceDefaults + Api + Web projects, .sln) | Kaylee | — | — |
| 2 | EF Core DbContext + entities + initial migration | Kaylee | 1 | 3, 4 |
| 3 | Vite React TS app scaffold inside RecipeHub.Web | Inara | 1 | 2, 4 |
| 4 | AppHost wiring: Api project + ViteApp resource | Kaylee | 1 | 2, 3 |
| 5 | Seed data implementation (12 recipes, 10 tags, ~69 steps) | Kaylee | 2 | 6 |
| 6 | Recipe CRUD endpoints | Kaylee | 2 | 5, 7 |
| 7 | Tag endpoints | Kaylee | 2 | 5, 6 |
| 8 | API client module (typed fetch wrappers) | Inara | 6, 7 | 9 |
| 9 | UI components: Button, Card, Spinner, Badge | Inara | 3 | 8 |
| 10 | HomePage + RecipeListPage + RecipeDetailPage | Inara | 8, 9 | 11 |
| 11 | RecipeEditPage (create/edit form) | Inara | 8, 9 | 10 |
| 12 | Cook Mode endpoints (with BUG 1) | Kaylee | 6 | 13 |
| 13 | useCookMode hook + CookModePage (with BUG 1) | Inara | 10, 12 | 14 |
| 14 | Search endpoints (with BUG 2) | Kaylee | 6 | 13 |
| 15 | SearchBar + FilterPanel + useSearch | Inara | 14 | 16 |
| 16 | Share endpoints (with BUG 3) | Kaylee | 6 | 15 |
| 17 | ShareButton + SharedRecipePage | Inara | 16 | 18 |
| 18 | Favorite endpoint stubs (501) | Kaylee | 6 | 17 |
| 19 | FavoritesPage shell ("Coming Soon") | Inara | 3 | 18 |
| 20 | xUnit test project setup (no tests) | Zoe | 1 | 19 |
| 21 | Vitest config setup (no tests) | Zoe | 3 | 20 |
| 22 | Skipped bug test files (BUG-001, BUG-002, BUG-003) | Zoe | 12, 14, 16, 20, 21 | — |
| 23 | .devcontainer configuration | Kaylee | 1 | 22 |
| 24 | README + formatting inconsistencies (2-3 for Ch03) | Mal | 22, 23 | — |
| 25 | End-to-end verification + bug reproduction checklist | Mal | 24 | — |

---

## 8. Open Questions for Zack

| # | Question | Recommendation |
|---|----------|----------------|
| 1 | **Node.js version?** | 22 LTS (current LTS, matches Codespaces default) |
| 2 | **Aspire dashboard auth in Codespaces?** | None for local dev — SRD §10 doesn't mention it, dashboard is dev-time only |
| 3 | **Package manager for frontend?** | npm (default for `AddViteApp()`, simplest for hackathon) |
| 4 | **Should we use Community Toolkit SQLite integration?** | No — file-based SQLite needs no orchestration; simpler to configure in appsettings.json |

---

## 9. Risks & Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| .NET 10 + Aspire 13.2 preview instability | High | Pin SDK in `global.json`, test in Codespaces before delivery |
| `AddViteApp()` port conflicts | Med | Don't call `.WithHttpEndpoint()` on Vite resources (per Aspire docs) |
| Planted bugs accidentally fixed by formatters | Med | Verify bugs survive `dotnet format` + `npm run lint --fix` (Phase 3 exit criteria) |
| Participants discover bugs before Challenge 05 | Low | Acceptable — they get a head start; bugs are in secondary features not exercised in Ch00–04 |
| React 19 breaking changes | Low | Pin exact version in `package.json` |
| Aspire JavaScript integration is relatively new | Med | Tested in Aspire 13.0+; use stable patterns from docs |

**Hackathon-specific trade-offs:**

1. **No auth** — SRD §4.2 specifies `UserId` as arbitrary string. Adding auth would triple complexity for no teaching benefit.

2. **No component library** — Hand-written CSS keeps bundle small and code readable. Participants can understand everything.

3. **SQLite over Postgres** — Zero setup, file-based, perfect for Codespaces. Performance is irrelevant at 12-recipe scale.

4. **Aspire SQLite resource skipped** — The Community Toolkit package exists but adds orchestration complexity. Direct appsettings.json config is clearer for learners.

---

## References

- SRD: `docs/solution-design.md`
- Data Assessment: `docs/data-assessment.md`
- Aspire JavaScript integration: `aspire-get_doc("javascript-integration")`
- Aspire SQLite integration: `aspire-get_doc("get-started-with-the-sqlite-integrations")`
- User directive: `.squad/decisions/inbox/copilot-directive-aspire-13-2.md`
