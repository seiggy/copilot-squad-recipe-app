# Solution Design — RecipeHub Starter Application

## 1. Executive Summary

RecipeHub is a full-stack recipe management application built with .NET 10 Minimal API and React 19. It serves as the "project under development" during the **GitHub Copilot & Squad Developer Workflow Hackathon** — a 4-hour, L400-level event where developers learn to integrate AI agents into their daily workflow.

The application is intentionally designed with three dormant bugs in secondary features (Cook Mode, Search, Sharing). These bugs do not block the primary hackathon flow (Challenges 00–04) but become the focus of Challenge 05 (Break-Fix), where participants use Copilot agents to diagnose and fix real production-style issues.

**Key characteristics:**

- Local-only application (no cloud deployment)
- SQLite database with seeded sample data
- Fully functional recipe CRUD as the baseline
- Three secondary features with purposeful defects
- Codespaces-ready with pre-configured dev container

---

## 2. Application Overview

### 2.1 Purpose

A recipe browsing, creation, and sharing application that gives hackathon participants a realistic codebase to work with. The app is complex enough to exercise multi-agent workflows (backend agent, frontend agent, tester agent) while staying small enough to understand in under 20 minutes.

### 2.2 Target Users

Hackathon participants: developers comfortable with C# and TypeScript who are learning GitHub Copilot and Squad.

### 2.3 Key Workflows

| Workflow | Status in Starter | Challenge |
|----------|-------------------|-----------|
| Browse recipes (paginated list) | ✅ Working | Ch00 |
| View recipe detail (ingredients, steps, tags) | ✅ Working | Ch00 |
| Create / edit / delete recipes | ✅ Working | Ch00 |
| Cook Mode (step-by-step walkthrough) | 🐛 Buggy (off-by-one) | Ch05 |
| Search and filter recipes | 🐛 Buggy (case-sensitive multi-word) | Ch05 |
| Share recipe via token link | 🐛 Buggy (tokens never persist) | Ch05 |
| Favorite / unfavorite recipes | 🔲 Stub (built by students in Ch02) | Ch02 |

---

## 3. Technical Architecture

### 3.1 Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Backend API | .NET 10 Minimal API | .NET 10 |
| ORM | Entity Framework Core | 10.x |
| Database | SQLite | via Microsoft.EntityFrameworkCore.Sqlite |
| Frontend | React 19 + TypeScript | React 19.x |
| Build Tool | Vite | 6.x |
| Routing | React Router | v7 |
| Server State | TanStack Query | v5 |
| Dev Container | GitHub Codespaces / Docker | - |
| Package Managers | NuGet (backend), npm (frontend) | - |

### 3.2 Project Structure

```
recipe-app/
├── api/                              # .NET 10 Minimal API
│   ├── Program.cs                    # App config, DI, endpoint registration
│   ├── appsettings.json              # Connection string, CORS origins
│   ├── Data/
│   │   ├── RecipeDbContext.cs         # EF Core DbContext + model config
│   │   └── SeedData.cs               # 12 seed recipes with steps and tags
│   ├── Models/
│   │   ├── Recipe.cs                 # Core recipe entity
│   │   ├── RecipeStep.cs             # Ordered cooking steps
│   │   ├── Tag.cs                    # Category tags
│   │   ├── RecipeTag.cs              # Many-to-many join
│   │   ├── Favorite.cs              # User favorites (stub for Ch02)
│   │   └── ShareToken.cs            # Share link tokens
│   ├── Endpoints/
│   │   ├── RecipeEndpoints.cs        # CRUD operations
│   │   ├── TagEndpoints.cs           # Tag listing and filtered browse
│   │   ├── CookModeEndpoints.cs      # Step-by-step walkthrough (🐛 BUG 1)
│   │   ├── SearchEndpoints.cs        # Full-text search (🐛 BUG 2)
│   │   ├── ShareEndpoints.cs         # Token-based sharing (🐛 BUG 3)
│   │   └── FavoriteEndpoints.cs      # Stubs — students implement in Ch02
│   └── Dtos/
│       ├── RecipeDto.cs              # List view DTO
│       ├── RecipeDetailDto.cs        # Detail view with steps and tags
│       ├── CookModeDto.cs            # Cook mode response
│       ├── CreateRecipeRequest.cs    # POST/PUT request body
│       └── ShareDto.cs               # Share token response
├── client/                           # React 19 + TypeScript + Vite
│   ├── index.html
│   ├── vite.config.ts                # Dev proxy: /api → localhost:5062
│   ├── package.json
│   ├── tsconfig.json
│   ├── src/
│   │   ├── main.tsx                  # App bootstrap with providers
│   │   ├── App.tsx                   # Root layout with navigation
│   │   ├── api/
│   │   │   └── client.ts             # Typed fetch wrappers for all endpoints
│   │   ├── components/
│   │   │   ├── ui/                   # Button, Card, Spinner, Badge, Timer
│   │   │   ├── recipe/               # RecipeCard, RecipeList, RecipeForm
│   │   │   ├── cook-mode/            # CookModeView, StepDisplay, StepTimer, StepNav
│   │   │   └── search/               # SearchBar, FilterPanel, SearchResults
│   │   ├── hooks/
│   │   │   ├── useRecipes.ts         # TanStack Query wrappers
│   │   │   ├── useCookMode.ts        # Cook mode state machine (🐛 BUG 1)
│   │   │   ├── useTimer.ts           # Countdown timer hook
│   │   │   └── useSearch.ts          # Search with debounce
│   │   ├── pages/
│   │   │   ├── HomePage.tsx
│   │   │   ├── RecipeListPage.tsx
│   │   │   ├── RecipeDetailPage.tsx
│   │   │   ├── RecipeEditPage.tsx
│   │   │   ├── CookModePage.tsx
│   │   │   ├── SharedRecipePage.tsx
│   │   │   └── FavoritesPage.tsx     # Shell — students build in Ch02
│   │   └── types/
│   │       └── recipe.ts             # Shared TypeScript interfaces
│   └── public/
│       └── favicon.svg
├── .devcontainer/
│   ├── devcontainer.json             # Codespaces configuration
│   └── Dockerfile                    # .NET 10 + Node.js 20 + CLI tools
├── .editorconfig
├── .gitignore
└── README.md                         # Getting started guide
```

### 3.3 Layered Architecture

```
┌──────────────────────────────────────────────────────────┐
│  React 19 SPA (Vite, port 5173)                          │
│  ┌─────────┐  ┌───────────┐  ┌─────────────┐            │
│  │ Pages   │→ │ Hooks     │→ │ API Client  │──┐         │
│  │ (routes)│  │ (TanStack │  │ (fetch)     │  │         │
│  └─────────┘  │  Query)   │  └─────────────┘  │         │
│               └───────────┘                    │         │
└────────────────────────────────────────────────┼─────────┘
                                                 │ HTTP (Vite proxy)
┌────────────────────────────────────────────────┼─────────┐
│  .NET 10 Minimal API (Kestrel, port 5062)      │         │
│  ┌────────────────┐  ┌─────────────────────┐   │         │
│  │ Endpoint Groups│→ │ EF Core DbContext   │   │         │
│  │ (MapGroup)     │  │ (SQLite provider)   │   │         │
│  └────────────────┘  └──────────┬──────────┘   │         │
│                                 │              │         │
│  ┌──────────────────────────────▼──────────┐   │         │
│  │ SQLite (recipes.db, file-based)         │   │         │
│  └─────────────────────────────────────────┘   │         │
└────────────────────────────────────────────────┴─────────┘
```

**API Layer**: Uses .NET 10 Minimal API patterns — `MapGroup` for endpoint organization, `TypedResults` for strongly-typed responses, automatic parameter binding from route/query/body, and built-in validation via `AddValidation()` with DataAnnotations on request records.

**Data Layer**: EF Core 10 with the SQLite provider. Migrations managed via `dotnet ef`. Seeding uses `UseSeeding`/`UseAsyncSeeding` (EF 9+ pattern) to populate 12 recipes on first run.

**Frontend Layer**: React 19 SPA with TanStack Query v5 for all server state (caching, refetching, pagination). React Router v7 for client-side routing. Vite dev server proxies `/api/*` to the .NET backend, avoiding CORS complexity during development.

---

## 4. Data Model

### 4.1 Entity Relationship Diagram

```
┌──────────────┐       ┌──────────────┐       ┌──────────┐
│   Recipe     │1────*│  RecipeStep   │       │   Tag    │
│──────────────│       │──────────────│       │──────────│
│ Id (PK)      │       │ Id (PK)      │       │ Id (PK)  │
│ Title        │       │ RecipeId (FK)│       │ Name     │
│ Description  │       │ StepNumber   │       └────┬─────┘
│ PrepTimeMins │       │ Instruction  │            │
│ CookTimeMins │       │ TimerMinutes │       ┌────┴─────┐
│ Servings     │       └──────────────┘       │RecipeTag │
│ Difficulty   │                              │──────────│
│ ImageUrl     │1─────────────────────────────*│RecipeId  │
│ CreatedAt    │                              │ TagId    │
│ UpdatedAt    │                              └──────────┘
│              │
│              │1────*┌──────────────┐
│              │      │ ShareToken   │
│              │      │──────────────│
│              │      │ Id (PK)      │
│              │      │ RecipeId (FK)│
│              │      │ Token        │
│              │      │ CreatedAt    │
│              │      │ ExpiresAt    │
│              │      └──────────────┘
│              │
│              │1────*┌──────────────┐
│              │      │ Favorite     │
└──────────────┘      │──────────────│
                      │ Id (PK)      │
                      │ UserId       │
                      │ RecipeId (FK)│
                      │ CreatedAt    │
                      └──────────────┘
```

### 4.2 Entity Specifications

#### Recipe

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| Title | string | Required, max 200 chars | |
| Description | string? | Optional, max 2000 chars | |
| PrepTimeMinutes | int | Range 1–480 | |
| CookTimeMinutes | int | Range 1–480 | |
| Servings | int | Range 1–20 | |
| Difficulty | Difficulty (enum) | Easy = 0, Medium = 1, Hard = 2 | Serialized as string in JSON |
| ImageUrl | string? | Optional | Placeholder URLs for seed data |
| CreatedAt | DateTime | UTC, set on creation | |
| UpdatedAt | DateTime | UTC, set on creation and update | |

#### RecipeStep

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| RecipeId | int | FK → Recipe, cascade delete | |
| StepNumber | int | 1-indexed, unique per recipe | |
| Instruction | string | Required, max 1000 chars | |
| TimerMinutes | int? | Nullable, range 1–120 when set | Only for timed steps (e.g., "bake for 25 minutes") |

Unique constraint: `(RecipeId, StepNumber)`

#### Tag

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| Name | string | Required, unique, max 50 chars | Seeded: Breakfast, Lunch, Dinner, Dessert, Vegetarian, Vegan, Quick, Italian, Asian, Mexican |

#### RecipeTag (Join Table)

| Property | Type | Constraints |
|----------|------|-------------|
| RecipeId | int | FK → Recipe, composite PK |
| TagId | int | FK → Tag, composite PK |

#### ShareToken

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| RecipeId | int | FK → Recipe | |
| Token | string | Required, unique | GUID without hyphens (32 chars) |
| CreatedAt | DateTime | UTC | |
| ExpiresAt | DateTime | UTC | Default: 7 days from creation |

#### Favorite (Stub — Students Build in Ch02)

| Property | Type | Constraints | Notes |
|----------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| UserId | string | Required | Simple string identifier (no auth system) |
| RecipeId | int | FK → Recipe | |
| CreatedAt | DateTime | UTC | |

Unique constraint: `(UserId, RecipeId)`

The `Favorite` model class exists in the starter but the `FavoriteEndpoints.cs` file contains only stubs that return `501 Not Implemented`. Students implement the full CRUD in Challenge 02.

---

## 5. API Endpoints

### 5.1 Recipe CRUD

#### GET /api/recipes

List recipes with pagination.

| Parameter | Source | Type | Default | Description |
|-----------|--------|------|---------|-------------|
| page | Query | int | 1 | Page number (1-indexed) |
| pageSize | Query | int | 10 | Items per page (max 50) |

**Response 200:**
```json
{
  "items": [
    {
      "id": 1,
      "title": "Classic Margherita Pizza",
      "description": "A simple, traditional Italian pizza...",
      "prepTimeMinutes": 30,
      "cookTimeMinutes": 15,
      "servings": 4,
      "difficulty": "Medium",
      "imageUrl": "/images/margherita.jpg",
      "tags": ["Italian", "Dinner"]
    }
  ],
  "totalCount": 12,
  "page": 1,
  "pageSize": 10
}
```

#### GET /api/recipes/{id}

Get recipe detail with steps and tags.

**Response 200:** `RecipeDetailDto` — includes `steps` array ordered by `stepNumber` and `tags` array.

**Response 404:** Recipe not found.

#### POST /api/recipes

Create a new recipe.

**Request body:**
```json
{
  "title": "Overnight Oats",
  "description": "Easy no-cook breakfast...",
  "prepTimeMinutes": 10,
  "cookTimeMinutes": 0,
  "servings": 2,
  "difficulty": "Easy",
  "steps": [
    { "stepNumber": 1, "instruction": "Combine oats and milk in a jar", "timerMinutes": null },
    { "stepNumber": 2, "instruction": "Refrigerate overnight", "timerMinutes": 480 }
  ],
  "tagIds": [1, 5]
}
```

**Response 201:** Created with Location header `/api/recipes/{id}`.

**Response 400:** Validation errors as ProblemDetails.

#### PUT /api/recipes/{id}

Update an existing recipe. Same body as POST.

**Response 200:** Updated `RecipeDetailDto`.

**Response 404:** Recipe not found.

#### DELETE /api/recipes/{id}

**Response 204:** Deleted successfully.

**Response 404:** Recipe not found.

### 5.2 Tags

#### GET /api/tags

**Response 200:** Array of `{ id, name }` objects.

#### GET /api/tags/{id}/recipes

Recipes filtered by tag. Same pagination as GET /api/recipes.

### 5.3 Cook Mode

#### GET /api/recipes/{id}/cook

Returns the recipe formatted for Cook Mode — title, total steps, and the full ordered step list.

**Response 200:**
```json
{
  "recipeId": 1,
  "recipeTitle": "Classic Margherita Pizza",
  "totalSteps": 6,
  "steps": [
    { "stepNumber": 1, "instruction": "Preheat oven to 475°F (245°C)", "timerMinutes": null },
    { "stepNumber": 2, "instruction": "Roll out pizza dough on floured surface", "timerMinutes": null },
    { "stepNumber": 3, "instruction": "Spread sauce evenly, leaving 1-inch border", "timerMinutes": null },
    { "stepNumber": 4, "instruction": "Add fresh mozzarella slices", "timerMinutes": null },
    { "stepNumber": 5, "instruction": "Bake until crust is golden and cheese bubbles", "timerMinutes": 12 },
    { "stepNumber": 6, "instruction": "Top with fresh basil leaves and drizzle olive oil", "timerMinutes": null }
  ]
}
```

#### GET /api/recipes/{id}/cook/steps/{stepNumber}

Returns a single step by number (1-indexed).

**Response 200:** Single step object.

**Response 404:** Recipe or step not found.

**🐛 BUG 1 lives here** — see Section 8.1.

### 5.4 Search

#### GET /api/recipes/search

| Parameter | Source | Type | Default | Description |
|-----------|--------|------|---------|-------------|
| q | Query | string | (required) | Search query |
| tags | Query | string | null | Comma-separated tag names |
| difficulty | Query | string | null | Easy, Medium, or Hard |
| maxTime | Query | int? | null | Max total time (prep + cook) in minutes |

**Response 200:** Array of `RecipeDto` matching the filters.

**🐛 BUG 2 lives here** — see Section 8.2.

### 5.5 Share

#### POST /api/recipes/{id}/share

Generate a shareable token link for a recipe.

**Response 200:**
```json
{
  "token": "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
  "shareUrl": "/shared/a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
  "expiresAt": "2026-04-16T20:29:10Z"
}
```

**Response 404:** Recipe not found.

**🐛 BUG 3 lives here** — see Section 8.3.

#### GET /api/shared/{token}

Retrieve a shared recipe by token.

**Response 200:** Full `RecipeDetailDto`.

**Response 404:** Token not found or expired.

### 5.6 Favorites (Stubs)

These endpoints return `501 Not Implemented` in the starter. Students build them in Challenge 02.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/favorites?userId={userId} | List user's favorites |
| POST | /api/favorites | Add a favorite |
| DELETE | /api/favorites/{recipeId}?userId={userId} | Remove a favorite |

---

## 6. Frontend Screens

### 6.1 Route Map

| Route | Page Component | Description |
|-------|---------------|-------------|
| `/` | HomePage | Welcome hero, featured recipes (random 3), category quick links |
| `/recipes` | RecipeListPage | Paginated card grid, search bar, tag filter chips |
| `/recipes/:id` | RecipeDetailPage | Full recipe: metadata, ingredient list, step list, Cook Mode button, Share button |
| `/recipes/new` | RecipeEditPage | Create form with validation |
| `/recipes/:id/edit` | RecipeEditPage | Edit form, pre-populated |
| `/recipes/:id/cook` | CookModePage | Step-by-step walkthrough with timers and progress bar |
| `/shared/:token` | SharedRecipePage | Read-only recipe view (fetched by share token) |
| `/favorites` | FavoritesPage | Shell with "Coming Soon" message — students build in Ch02 |

### 6.2 Component Hierarchy

```
App
├── Navigation (logo, links: Home, Recipes, Favorites)
├── Routes
│   ├── HomePage
│   │   ├── HeroSection
│   │   └── FeaturedRecipes → RecipeCard[]
│   ├── RecipeListPage
│   │   ├── SearchBar
│   │   ├── FilterPanel (tags, difficulty, max time)
│   │   ├── RecipeCard[] (grid)
│   │   └── Pagination
│   ├── RecipeDetailPage
│   │   ├── RecipeHeader (title, meta, difficulty badge)
│   │   ├── StepList
│   │   ├── TagList
│   │   ├── CookModeButton → navigates to /recipes/:id/cook
│   │   └── ShareButton → calls POST /api/recipes/:id/share
│   ├── CookModePage
│   │   ├── ProgressBar (step X of Y)
│   │   ├── StepDisplay (large text, current instruction)
│   │   ├── StepTimer (countdown when step has timerMinutes)
│   │   └── StepNavigation (Previous / Next buttons)
│   ├── RecipeEditPage
│   │   └── RecipeForm (controlled form with dynamic step list)
│   ├── SharedRecipePage
│   │   └── RecipeDetailView (read-only)
│   └── FavoritesPage
│       └── "Coming Soon" placeholder
└── Footer
```

### 6.3 Cook Mode UX Design

Cook Mode provides a distraction-free, step-by-step cooking experience:

```
┌─────────────────────────────────────────┐
│  RecipeHub  ·  Cook Mode                │
├─────────────────────────────────────────┤
│                                         │
│  Classic Margherita Pizza               │
│  Step 5 of 6   ●●●●●○                  │
│                                         │
│  ┌─────────────────────────────────┐    │
│  │                                 │    │
│  │  Bake until crust is golden     │    │
│  │  and cheese bubbles             │    │
│  │                                 │    │
│  └─────────────────────────────────┘    │
│                                         │
│       ┌──────────────────┐              │
│       │   ⏱ 12:00        │              │
│       │  [ Start Timer ] │              │
│       └──────────────────┘              │
│                                         │
│  [ ← Previous ]      [ Next Step → ]   │
│                                         │
│  [ ✕ Exit Cook Mode ]                  │
└─────────────────────────────────────────┘
```

**Key behaviors:**
- Large, readable typography (18–24px body text)
- Progress indicator (dots or bar) showing current position
- Per-step countdown timer (only displayed when `timerMinutes` is set)
- Timers continue running when navigating between steps
- Screen wake lock via `navigator.wakeLock` API
- Previous/Next navigation with boundary checks
- Exit button returns to recipe detail page

---

## 7. Seed Data

The database ships with 12 seed recipes across diverse categories:

| # | Title | Difficulty | Prep | Cook | Tags | Steps | Timed Steps |
|---|-------|-----------|------|------|------|-------|-------------|
| 1 | Classic Margherita Pizza | Medium | 30 | 15 | Italian, Dinner | 6 | 1 (bake 12m) |
| 2 | Fluffy Pancakes | Easy | 10 | 15 | Breakfast, Quick | 5 | 1 (cook 3m) |
| 3 | Chicken Alfredo Pasta | Medium | 15 | 25 | Italian, Dinner | 7 | 2 (boil 10m, simmer 5m) |
| 4 | Thai Green Curry | Medium | 20 | 20 | Asian, Dinner | 6 | 1 (simmer 15m) |
| 5 | Avocado Toast | Easy | 5 | 3 | Breakfast, Vegetarian, Quick | 4 | 1 (toast 3m) |
| 6 | Beef Tacos | Easy | 15 | 10 | Mexican, Dinner | 5 | 1 (cook 8m) |
| 7 | Chocolate Lava Cake | Hard | 20 | 14 | Dessert | 8 | 1 (bake 14m) |
| 8 | Vegetable Stir-Fry | Easy | 15 | 10 | Asian, Vegetarian, Vegan, Quick | 5 | 1 (stir-fry 5m) |
| 9 | French Onion Soup | Hard | 15 | 60 | Dinner | 7 | 2 (caramelize 45m, broil 3m) |
| 10 | Berry Smoothie Bowl | Easy | 10 | 0 | Breakfast, Vegetarian, Quick | 4 | 0 |
| 11 | Homemade Sushi Rolls | Hard | 45 | 20 | Asian, Dinner | 8 | 1 (cook rice 20m) |
| 12 | Tiramisu | Medium | 30 | 0 | Italian, Dessert | 6 | 0 |

**Seed tags**: Breakfast, Lunch, Dinner, Dessert, Vegetarian, Vegan, Quick, Italian, Asian, Mexican

---

## 8. Purposeful Bugs — Challenge 05 Specification

Three bugs are planted in the starter application. They exist in secondary features that students can observe but that do not block the core hackathon flow (recipe CRUD, favorites, quality gates, test coverage).

Each bug is described from four perspectives:
1. **User-visible symptom** — what a student would report
2. **Root cause** — the actual code defect
3. **Fix** — the correct resolution
4. **Regression test** — what to verify after fixing

---

### 8.1 Bug 1: Cook Mode Skips First Step (Off-By-One)

**Difficulty:** Medium

**Feature:** Cook Mode step-by-step walkthrough

**Symptom:** When a user enters Cook Mode for any recipe, they immediately see Step 2. Pressing "Previous" from Step 2 shows a blank/error state. The last step of the recipe is unreachable — pressing "Next" on the second-to-last step does nothing.

**Root Cause — Backend** (`api/Endpoints/CookModeEndpoints.cs`):

The single-step endpoint applies an incorrect offset:

```csharp
group.MapGet("/{id}/cook/steps/{stepNumber}", async (int id, int stepNumber, RecipeDbContext db) =>
{
    var recipe = await db.Recipes
        .Include(r => r.Steps)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (recipe is null) return TypedResults.NotFound();

    // BUG: Subtracts 1 from an already 1-indexed stepNumber
    // Step 1 looks for StepNumber == 0, which doesn't exist
    // Step 2 looks for StepNumber == 1, which is actually step 1
    var step = recipe.Steps.FirstOrDefault(s => s.StepNumber == stepNumber - 1);

    if (step is null) return TypedResults.NotFound();

    return TypedResults.Ok(new CookStepDto(step.StepNumber, step.Instruction, step.TimerMinutes));
});
```

**Root Cause — Frontend** (`client/src/hooks/useCookMode.ts`):

The hook initializes the current step to 1 instead of 0:

```typescript
export function useCookMode(steps: CookStep[]) {
  // BUG: Starting at index 1 skips the first element in the steps array
  const [currentStepIndex, setCurrentStepIndex] = useState(1);

  const goToNext = () => {
    setCurrentStepIndex(prev => Math.min(prev + 1, steps.length - 1));
  };

  const goToPrevious = () => {
    setCurrentStepIndex(prev => Math.max(prev - 1, 0));
  };

  return {
    currentStep: steps[currentStepIndex],
    stepNumber: currentStepIndex + 1,     // Display is 1-indexed
    totalSteps: steps.length,
    goToNext,
    goToPrevious,
    isFirst: currentStepIndex === 0,
    isLast: currentStepIndex === steps.length - 1,
  };
}
```

**Fix — Backend:**

Remove the `- 1` offset. Steps in the database are 1-indexed; the URL parameter is also 1-indexed:

```csharp
var step = recipe.Steps.FirstOrDefault(s => s.StepNumber == stepNumber);
```

**Fix — Frontend:**

Initialize state to 0:

```typescript
const [currentStepIndex, setCurrentStepIndex] = useState(0);
```

**Regression Tests:**
- Cook Mode loads with step 1 displayed first
- Every step from 1 to N is reachable via Next
- Previous from step 1 is disabled or stays on step 1
- Last step is reachable and Next is disabled on it
- Step timer displays for timed steps on first and last positions

---

### 8.2 Bug 2: Search Fails on Multi-Word and Mixed-Case Queries

**Difficulty:** Medium-Hard

**Feature:** Recipe search

**Symptom:** Searching "Chicken Alfredo Pasta" (exact case) works. But searching "chicken pasta" (lowercase) returns 0 results. Searching "PIZZA" returns 0 results even though "Classic Margherita Pizza" exists. Single-word searches only work when the case matches the title exactly — "Chicken" finds results but "chicken" does not.

**Root Cause** (`api/Endpoints/SearchEndpoints.cs`):

Two interacting defects:

```csharp
group.MapGet("/search", async (string q, string? tags, string? difficulty,
    int? maxTime, RecipeDbContext db) =>
{
    var query = db.Recipes
        .Include(r => r.Tags)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
    {
        var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            // BUG 1: string.Contains() translates to SQLite's instr() function,
            // which is CASE-SENSITIVE. "chicken" won't match "Chicken".
            // BUG 2: Only searches Title, ignoring Description entirely.
            // "green curry" won't match a recipe whose Title is "Thai Green Curry"
            // if "curry" appears only in the description.
            query = query.Where(r => r.Title.Contains(word));
        }
    }

    // Tag filtering, difficulty filtering, and maxTime filtering work correctly
    if (!string.IsNullOrWhiteSpace(tags))
    {
        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
        query = query.Where(r => r.Tags.Any(t => tagList.Contains(t.Tag.Name)));
    }

    if (Enum.TryParse<Difficulty>(difficulty, true, out var diff))
    {
        query = query.Where(r => r.Difficulty == diff);
    }

    if (maxTime.HasValue)
    {
        query = query.Where(r => r.PrepTimeMinutes + r.CookTimeMinutes <= maxTime.Value);
    }

    var results = await query.Select(r => r.ToDto()).ToListAsync();
    return TypedResults.Ok(results);
});
```

**Fix:**

Replace `string.Contains` with `EF.Functions.Like` for case-insensitive matching, and search both Title and Description:

```csharp
foreach (var word in words)
{
    var pattern = $"%{word}%";
    query = query.Where(r =>
        EF.Functions.Like(r.Title, pattern) ||
        EF.Functions.Like(r.Description ?? "", pattern));
}
```

`EF.Functions.Like` translates to SQLite's `LIKE` operator, which is case-insensitive for ASCII characters (covers English recipe names).

**Regression Tests:**
- Multi-word search: "chicken pasta" finds "Chicken Alfredo Pasta"
- Case-insensitive: "PIZZA", "pizza", "Pizza" all return the same results
- Description search: query matching only in description still returns the recipe
- Single-word search still works correctly
- Tag and difficulty filters still work when combined with text search
- Empty search query returns all recipes (or appropriate error)

---

### 8.3 Bug 3: Share Links Always Return 404

**Difficulty:** Hard

**Feature:** Recipe sharing via token-based URLs

**Symptom:** A user clicks "Share Recipe" on a recipe detail page. The UI shows a share link (e.g., `/shared/a1b2c3d4...`). When anyone opens that link — including the person who just created it — they get a 404 "Not Found" page. No error is shown during share creation. The bug is invisible at creation time and only surfaces when someone tries to use the link.

**Root Cause** (`api/Endpoints/ShareEndpoints.cs`):

The token is assigned AFTER the database save, so the row is persisted with a null token:

```csharp
group.MapPost("/{id}/share", async (int id, RecipeDbContext db) =>
{
    var recipe = await db.Recipes.FindAsync(id);
    if (recipe is null) return TypedResults.NotFound();

    var share = new ShareToken
    {
        RecipeId = id,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(7)
        // NOTE: Token is NOT set here
    };

    db.ShareTokens.Add(share);
    await db.SaveChangesAsync();  // Saves row with Token = null (or empty default)

    // BUG: Token generated AFTER SaveChangesAsync — never persisted to database
    share.Token = Guid.NewGuid().ToString("N");
    // Missing: no second SaveChangesAsync() call

    return TypedResults.Ok(new ShareDto(
        share.Token,                                    // Returns valid-looking token to UI
        $"/shared/{share.Token}",
        share.ExpiresAt
    ));
});
```

The GET endpoint then queries by token, but since all saved tokens are null, no match is ever found:

```csharp
group.MapGet("/{token}", async (string token, RecipeDbContext db) =>
{
    var share = await db.ShareTokens
        .Include(s => s.Recipe)
            .ThenInclude(r => r.Steps.OrderBy(s => s.StepNumber))
        .Include(s => s.Recipe)
            .ThenInclude(r => r.RecipeTags)
                .ThenInclude(rt => rt.Tag)
        .FirstOrDefaultAsync(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);

    if (share is null) return TypedResults.NotFound();

    return TypedResults.Ok(share.Recipe.ToDetailDto());
});
```

**Why this is tricky to diagnose:**
- The POST endpoint returns 200 with a valid-looking token — no error
- The frontend displays the share link without issues
- The GET endpoint correctly handles the 404 case — it's not a crash
- The bug is a data persistence timing issue, not a logic error visible from the endpoint signatures
- Students need to check the actual database contents to see that Token columns are null

**Fix:**

Move the token generation before `SaveChangesAsync`:

```csharp
var share = new ShareToken
{
    RecipeId = id,
    Token = Guid.NewGuid().ToString("N"),  // Generate BEFORE save
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddDays(7)
};

db.ShareTokens.Add(share);
await db.SaveChangesAsync();  // Now saves the complete record with token
```

**Regression Tests:**
- Share token is non-null in database after POST
- GET /api/shared/{token} returns the correct recipe
- Expired tokens return 404
- Each share generates a unique token
- Sharing the same recipe twice creates two distinct tokens

---

## 9. Non-Functional Requirements

### 9.1 Performance

| Metric | Target |
|--------|--------|
| Application cold start | < 5 seconds |
| API response time (CRUD) | < 200ms |
| API response time (search) | < 500ms |
| Frontend initial load (dev) | < 3 seconds |
| Hot Module Replacement | < 500ms |

These targets are for local development on a modern laptop or GitHub Codespaces (4-core).

### 9.2 Compatibility

| Environment | Support Level |
|-------------|--------------|
| GitHub Codespaces (Linux) | Primary — dev container pre-configured |
| Local Docker (Dev Container) | Supported — same devcontainer.json |
| macOS (bare metal) | Supported — manual prerequisite install |
| Windows (bare metal) | Supported — manual prerequisite install |
| Linux (bare metal) | Supported — manual prerequisite install |

### 9.3 Database

- SQLite file-based (`recipes.db` in the `api/` directory)
- Auto-created on first run via EF Core migrations
- Seed data applied via `UseSeeding` / `UseAsyncSeeding`
- Reset by deleting `recipes.db` and restarting, or `dotnet ef database drop`

### 9.4 CORS

Configured in `Program.cs` for the Vite dev server:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

In production (Codespaces), the Vite proxy handles this, so CORS is a fallback for direct API access.

### 9.5 Error Handling

- Global exception handler returns `ProblemDetails` (RFC 9457)
- Validation errors return 400 with detailed field-level errors
- Not Found returns 404 with empty body (or ProblemDetails)
- Unhandled exceptions return 500 with generic ProblemDetails (no stack trace)

### 9.6 Serialization

- JSON property naming: camelCase (`JsonNamingPolicy.CamelCase`)
- Enums serialized as strings (`JsonStringEnumConverter`)
- DateTime serialized in UTC with Z suffix
- Null properties included in response (not omitted)

---

## 10. Development Environment

### 10.1 Dev Container Configuration

The `.devcontainer/devcontainer.json` configures:

| Component | Details |
|-----------|---------|
| Base image | `mcr.microsoft.com/devcontainers/dotnet:10.0` |
| Node.js | Feature: `ghcr.io/devcontainers/features/node:1` (v20 LTS) |
| GitHub CLI | Feature: `ghcr.io/devcontainers/features/github-cli:1` |
| Copilot CLI | Post-create: `gh extension install github/gh-copilot` |
| Squad CLI | Post-create: `npm install -g @bradygaster/squad-cli` |
| VS Code Extensions | C# Dev Kit, ESLint, Prettier, Copilot, Copilot Chat |
| Port forwarding | 5062 (API), 5173 (Vite) |

### 10.2 Running the Application

**API (terminal 1):**
```bash
cd api
dotnet watch run
# Starts on http://localhost:5062
```

**Frontend (terminal 2):**
```bash
cd client
npm install
npm run dev
# Starts on http://localhost:5173, proxies /api to :5062
```

### 10.3 Database Management

```bash
cd api
dotnet ef migrations add <MigrationName>    # Create migration
dotnet ef database update                    # Apply migrations
dotnet ef database drop                      # Reset database
```

Deleting `recipes.db` and restarting the API also resets with fresh seed data.

---

## 11. Hackathon Integration Points

This section maps how the starter app connects to each hackathon challenge.

### Challenge 00 — Base Camp

Students verify the environment by:
1. Running `dotnet build` in `api/` — should succeed
2. Running `npm install && npm run dev` in `client/` — should succeed
3. Browsing to `http://localhost:5173` — should see the recipe list
4. Hitting `http://localhost:5062/api/recipes` — should return JSON

**App requirement:** Recipe CRUD and listing must work. The 3 bugs in Cook Mode, Search, and Share do not affect this verification.

### Challenge 01 — Assemble Your Squad

Students run `squad init` in the project root. The app itself is unchanged — this challenge configures the AI agent team.

**App requirement:** The project structure must be clear enough for Squad to generate sensible agent charters (backend C# agent, frontend React agent, etc.).

### Challenge 02 — Ship a Feature (Favorites)

Students delegate the "favorites" feature to their Squad. The agents build:
- Backend: `FavoriteEndpoints.cs` (replace stubs with real CRUD)
- Frontend: Favorite toggle on `RecipeCard`, `FavoritesPage` content

**App requirement:** The `Favorite` model exists. `FavoriteEndpoints.cs` has stub endpoints returning 501. `FavoritesPage.tsx` has a shell with "Coming Soon" text. The `RecipeDbContext` includes `DbSet<Favorite>`.

### Challenge 03 — Quality Gates

Students add Husky git hooks and Copilot hooks. They need existing code with minor formatting inconsistencies for the hooks to catch.

**App requirement:** Include 2-3 minor formatting issues in the starter (e.g., inconsistent indentation in one file, a missing semicolon that ESLint would flag). These should NOT prevent compilation.

### Challenge 04 — Test Coverage Blitz

Students generate tests targeting 80%+ coverage across both stacks.

**App requirement:** The `api/` project must be testable with xUnit. The `client/` project must have Vitest and Testing Library configured. No existing tests in the starter (students create them all).

### Challenge 05 — Break-Fix

Students discover the 3 bugs documented in Section 8. The challenge presents them as user reports:

1. "Users are saying Cook Mode starts on the wrong step and they can't reach the last step."
2. "Searching for 'chicken pasta' returns nothing. Single words work fine. Some users say search is case-sensitive."
3. "Share links always show 404. Users create the link, send it, and the recipient gets nothing."

Students use Copilot agents to diagnose root causes, implement fixes, and write regression tests.

**App requirement:** All 3 bugs must be present, reproducible, and independent of each other.

### Challenge 06 — Autonomous Operations

Students configure Ralph, governance hooks, and Aspire Dashboard.

**App requirement:** The app needs no specific modifications for this challenge. Students create GitHub Issues and configure external tooling.

---

## 12. Testing Strategy

### 12.1 Backend (xUnit + EF Core InMemory)

The starter ships with a test project skeleton (`api.Tests/`) but no test files. Students generate tests in Challenge 04.

**Test targets:**
- Recipe CRUD endpoints (happy path + edge cases)
- Cook Mode endpoints (step retrieval, boundary cases)
- Search endpoint (multi-word, case sensitivity, filters)
- Share endpoints (create, retrieve, expiry)
- Input validation (empty titles, negative servings, out-of-range times)

**Test infrastructure:**
- `WebApplicationFactory<Program>` for integration tests
- SQLite in-memory for isolated test databases
- Seed data helper for consistent test fixtures

### 12.2 Frontend (Vitest + Testing Library)

The starter ships with Vitest configured in `vite.config.ts` but no test files.

**Test targets:**
- RecipeCard rendering (props, click handlers)
- RecipeForm validation (required fields, ranges)
- Cook Mode navigation (step progression, timer display)
- Search interaction (debounce, filter application)
- API client functions (mock fetch, error handling)

### 12.3 Coverage Target

80% line coverage on both stacks, enforced by the challenge rubric.

---

## Appendix A: TypeScript Interfaces

```typescript
export interface Recipe {
  id: number;
  title: string;
  description: string | null;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: "Easy" | "Medium" | "Hard";
  imageUrl: string | null;
  tags: string[];
}

export interface RecipeDetail extends Recipe {
  steps: RecipeStep[];
  createdAt: string;
  updatedAt: string;
}

export interface RecipeStep {
  stepNumber: number;
  instruction: string;
  timerMinutes: number | null;
}

export interface CookModeData {
  recipeId: number;
  recipeTitle: string;
  totalSteps: number;
  steps: RecipeStep[];
}

export interface ShareData {
  token: string;
  shareUrl: string;
  expiresAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface Favorite {
  id: number;
  userId: string;
  recipeId: number;
  recipe: Recipe;
  createdAt: string;
}

export interface CreateRecipeRequest {
  title: string;
  description?: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: "Easy" | "Medium" | "Hard";
  steps: { stepNumber: number; instruction: string; timerMinutes?: number }[];
  tagIds: number[];
}
```

## Appendix B: C# DTOs

```csharp
public record RecipeDto(
    int Id,
    string Title,
    string? Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    string? ImageUrl,
    List<string> Tags
);

public record RecipeDetailDto(
    int Id,
    string Title,
    string? Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    string? ImageUrl,
    List<string> Tags,
    List<RecipeStepDto> Steps,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record RecipeStepDto(
    int StepNumber,
    string Instruction,
    int? TimerMinutes
);

public record CookModeDto(
    int RecipeId,
    string RecipeTitle,
    int TotalSteps,
    List<RecipeStepDto> Steps
);

public record CookStepDto(
    int StepNumber,
    string Instruction,
    int? TimerMinutes
);

public record CreateRecipeRequest(
    [Required, StringLength(200)] string Title,
    [StringLength(2000)] string? Description,
    [Range(1, 480)] int PrepTimeMinutes,
    [Range(1, 480)] int CookTimeMinutes,
    [Range(1, 20)] int Servings,
    Difficulty Difficulty,
    List<CreateStepRequest> Steps,
    List<int> TagIds
);

public record CreateStepRequest(
    int StepNumber,
    [Required, StringLength(1000)] string Instruction,
    [Range(1, 120)] int? TimerMinutes
);

public record ShareDto(
    string Token,
    string ShareUrl,
    DateTime ExpiresAt
);
```
