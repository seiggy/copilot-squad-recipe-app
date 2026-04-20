# Project Context

- **Owner:** Zack Way
- **Project:** RecipeHub — a .NET 10 + React 19 recipe-sharing application used as the demo/starter project for the GitHub Copilot & Squad Developer Workflow Hackathon.
- **Stack:** .NET 10 Minimal API, EF Core 10, SQLite, React 19, TypeScript, Vite 6, TanStack Query v5
- **Created:** 2026-04-20

## Core Context

- This is a **hackathon starter**. Clarity and teachability trump cleverness.
- Documentation lives in `docs/`: `executive-brief.md`, `solution-design.md` (SRD — source of truth for endpoints, models, and bug specs), `architecture-diagram.drawio`, `data-assessment.md`, `responsible-ai.md`, `cost-estimation.md`, `delivery-plan.md`.
- **Three planted bugs** exist intentionally in secondary features — do NOT fix unprompted:
  1. Cook Mode Off-By-One (first step skipped, last unreachable)
  2. Search Case Sensitivity (multi-word / mixed-case queries fail)
  3. Share Token Persistence (tokens never saved, links 404)
  Full specs in `docs/solution-design.md §8`. These are Challenge 05 material.
- Target environment: GitHub Codespaces (recommended) or local devcontainer.

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-20 — Item 1 scaffold pinned versions & gotchas (Kaylee)

**.NET SDK pinned in `global.json`:** `10.0.201` with `rollForward: latestFeature` (env also had `10.0.106`).

**Aspire 13.2 package versions pinned:**
- `Aspire.AppHost.Sdk` → `13.2.2` (SDK attribute on AppHost csproj)
- `Aspire.Hosting.AppHost` → `13.2.2`
- `Aspire.Hosting.JavaScript` → `13.2.2` (for `AddViteApp` in Item 4)
- ServiceDefaults (from `aspire-servicedefaults` template, Aspire 13.2 line):
  - `Microsoft.Extensions.Http.Resilience` 10.2.0
  - `Microsoft.Extensions.ServiceDiscovery` 10.2.0
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.15.0
  - `OpenTelemetry.Extensions.Hosting` 1.15.0
  - `OpenTelemetry.Instrumentation.AspNetCore` 1.15.0
  - `OpenTelemetry.Instrumentation.Http` 1.15.0
  - `OpenTelemetry.Instrumentation.Runtime` 1.15.0

**EF Core (Api project, for Item 2):**
- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.0
- `Microsoft.EntityFrameworkCore.Design` 10.0.0 (PrivateAssets=all)

**Gotchas:**
1. `dotnet new sln` on .NET 10 SDK creates `.slnx` (XML solution) by default. Task specified `.sln`, so we had to pass `--format sln` explicitly.
2. `aspire-apphost` template ships `AppHost.cs` (not `Program.cs`) and relies on the `Aspire.AppHost.Sdk` for implicit `Aspire.Hosting.AppHost` — added the package ref explicitly per task requirement; build succeeds with both.
3. ServiceDefaults template already maps `/health` and `/alive` via `MapDefaultEndpoints()` — no manual `/health` endpoint needed in Api.
4. `aspire-doctor` reports Docker warning (not running) — harmless for this item; required only when containers are introduced.
5. `RecipeHub.Web/` left as directory with `.gitkeep`; Inara will scaffold Vite app in Item 3.

**Commands that worked (from repo root):**
```
dotnet new globaljson --sdk-version 10.0.201 --roll-forward latestFeature
dotnet new sln -n RecipeHub --format sln
dotnet new aspire-apphost         -n RecipeHub.AppHost         -o src\RecipeHub.AppHost
dotnet new aspire-servicedefaults -n RecipeHub.ServiceDefaults -o src\RecipeHub.ServiceDefaults
dotnet new webapi                 -n RecipeHub.Api             -o src\RecipeHub.Api --framework net10.0
dotnet sln RecipeHub.sln add src\RecipeHub.AppHost\RecipeHub.AppHost.csproj src\RecipeHub.ServiceDefaults\RecipeHub.ServiceDefaults.csproj src\RecipeHub.Api\RecipeHub.Api.csproj
dotnet build RecipeHub.sln    # succeeded: 0 warnings, 0 errors, ~4.5s
```


### 2026-04-20 — Item 4 AppHost wiring + CORS (Kaylee)

**Aspire 13.2 `AddViteApp` signature used (confirmed via `aspire-get_doc javascript-integration`):**
- `builder.AddViteApp(string name, string workingDirectory, string? runScriptName = "dev")` returns `IResourceBuilder<ViteAppResource>`.
- Auto-registers an `http` endpoint and `PORT` env var — do NOT call `.WithHttpEndpoint()` on it (would duplicate endpoint).
- Chained `.WithReference(api)` injects service-discovery env vars for the Api resource; `.WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"))` surfaces the Api URL to Vite's client bundle (only `VITE_*`-prefixed vars reach `import.meta.env`).

**Surprises vs. Mal's sample Program.cs:** None — the 13.2 API matches the sample 1:1. The Mal sample stored the Vite resource in a `web` local; I dropped the unused local (no further references to it), otherwise identical.

**AppHost csproj:** Added `<ProjectReference Include="..\RecipeHub.Api\RecipeHub.Api.csproj" />` so the Aspire SDK code-gens `Projects.RecipeHub_Api`. Path uses Windows-style backslashes in the csproj (MSBuild normalizes).

**CORS approach (Api):** Development-only, permissive. Named policy `RecipeHubDevCors` uses `SetIsOriginAllowed(_ => true)` + `AllowAnyHeader` + `AllowAnyMethod` + `AllowCredentials`. Applied in the pipeline only when `app.Environment.IsDevelopment()`. This is intentionally loose for v1 — Aspire allocates the Vite dev port dynamically (5173, 5174, ...), so pinning origins is brittle. Production tightening is out of scope per the task brief.
  - Why not read a `VITE_*` env var for the origin? Aspire sets `VITE_API_BASE_URL` on the web resource (Vite-side), not on the Api resource. The Api doesn't know the Vite origin ahead of time; `SetIsOriginAllowed` dev-gated is the pragmatic answer.
  - `AllowAnyOrigin` + `AllowCredentials` is invalid per CORS spec — hence the `SetIsOriginAllowed` predicate pattern.

**Coordination note:** Item 2 (EF Core DbContext) landed concurrently in `Program.cs`; I left the `AddDbContext<RecipeDbContext>` call untouched and inserted CORS registration/use around it. No `Models/` or `Data/` edits.

**Build:** `dotnet build RecipeHub.sln` — 0 warnings, 0 errors, ~2.8s.


### 2026-04-20 — Item 2 EF Core DbContext + InitialCreate migration (Kaylee)

**EF Core 10 patterns used:**
- Primary-constructor DbContext (`public class RecipeDbContext(DbContextOptions<RecipeDbContext> options) : DbContext(options)`). Works cleanly on EF Core 10 / .NET 10.
- `HasConversion<string>()` on `Difficulty` — stores enum as TEXT in SQLite; readable in sqlite CLI.
- Explicit `ValueConverter<DateTime, DateTime>` on all DateTime columns to normalize to UTC on write and tag `DateTimeKind.Utc` on read. SQLite loses `DateTimeKind` otherwise.
- `OnDelete(DeleteBehavior.Cascade)` for Recipe → Steps / RecipeTags / ShareTokens / Favorites. SQLite honors FK cascades when `PRAGMA foreign_keys = ON` (Sqlite provider enables by default).
- Composite PK on `RecipeTag` via `HasKey(rt => new { rt.RecipeId, rt.TagId })`.
- Unique indexes: `Tag.Name`, `ShareToken.Token`, `(RecipeStep.RecipeId, StepNumber)`, `(Favorite.UserId, RecipeId)`.
- Data annotations (`[Required]`, `[MaxLength]`) used for column sizing; Fluent API reserved for keys, relationships, indexes, conversions — keeps Models readable.

**dotnet-ef version:** `10.0.0` pinned in `.config/dotnet-tools.json` (rollForward: false). Tool restored via `dotnet tool restore`; migration generated with `dotnet ef migrations add InitialCreate -o Data/Migrations`.

**.NET 10 preview quirks:**
1. `dotnet new tool-manifest` on this SDK writes `dotnet-tools.json` to the **current directory** rather than `.config/dotnet-tools.json`. Had to manually `Move-Item` it into `.config/`; `dotnet tool restore` then finds it correctly. (Noted so Zoe/Inara don't fight the same quirk.)
2. EF Core 10 migration designer emits the usual `[DbContext(typeof(RecipeDbContext))]` + snapshot pattern unchanged from EF 9.
3. Sqlite provider still stores DateTime as TEXT in ISO-8601; the UTC ValueConverter is required — without it `DateTimeKind.Unspecified` leaks out of queries.

**Migration contents verified:** 6 CreateTable calls (Recipes, Tags, Favorites, RecipeSteps, ShareTokens, RecipeTags), all FKs with cascade, and the 4 unique indexes above. Snapshot checked in.

**Program.cs delta:** single `AddDbContext<RecipeDbContext>` registration reading `ConnectionStrings:RecipeDb` from config (`Data Source=recipes.db` default). No `Database.Migrate()`, no `EnsureCreated()`, no seed — Item 5 owns seeding.

**Build:** `dotnet build RecipeHub.sln` — 0 warnings, 0 errors, ~1.6s.

### 2026-04-20 — Items 6 + 7 Recipe CRUD + Tag endpoints (Kaylee)

Shipped `RecipeEndpoints.cs` (GET list/detail, POST, PUT, DELETE) and `TagEndpoints.cs` (GET list with RecipeCount) plus six record DTOs under `Dtos/`. Tag-on-create uses match-only by Name (no auto-create) per SRD §5.2. Program.cs delta: one `using` + `app.MapRecipeEndpoints(); app.MapTagEndpoints();` after `MapDefaultEndpoints` — leaves room for Item 5's migrate+seed block. Used `TimerMinutes` on RecipeStepDto (not DurationSeconds) to match the model; omitted `Ingredients` (no such entity). Build: 0/0.

- 2026-04-20 — Item 5: Seed data (SeedData.EnsureSeeded) — 10 tags, 12 recipes, 71 steps, 26 recipe-tag joins; idempotent via db.Recipes.Any(); migrate+seed wired in Program.cs scope block. Build: 0W/0E.
- 2026-04-20 — Items 12+14+16+18: Cook Mode, Search, Share endpoints + Favorite 501 stubs. Planted bugs 1 (off-by-one stepNumber-1), 2 (case-sensitive Contains), 3 (Token assigned after SaveChangesAsync) exactly per spec. 6 files added (4 endpoints + 2 DTOs), Program.cs +4 lines. Build: 0W/0E.
- 2026-04-20 — Item 23: .devcontainer config — base image mcr.microsoft.com/devcontainers/dotnet:1-10.0, Node 22 LTS feature (container honors directive; host shipped on Node 24), github-cli + powershell features, 8 VS Code extensions (incl. Copilot + Copilot Chat), postCreate = dotnet restore + npm install (no AppHost auto-start), forwardPorts [5000, 5001, 17050] with labels. JSON validated via ConvertFrom-Json. Decision note: .squad/decisions/inbox/kaylee-item23-devcontainer.md.
