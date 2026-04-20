# Squad Decisions

## Active Decisions

### 2026-04-20T17:50:05Z: Use Aspire 13.2 as the base app template
**By:** Zack Way (via Copilot)
**What:** The RecipeHub solution MUST be scaffolded on .NET Aspire 13.2 as the base app template. The .NET 10 Minimal API backend, SQLite data, and React 19 + Vite frontend all orchestrate through the Aspire AppHost. Frontend is a Node.js resource projected by Aspire; backend is a project resource. Aspire owns service discovery, configuration, and dev-time dashboard.
**Why:** User directive — captured for team memory. Affects repo structure, run/debug story, and ServiceDefaults wiring.

### 2026-04-20T17:55:00Z: Build plan approved + open questions resolved
**By:** Zack Way (via Copilot)
**What:**
- Mal's build plan at .squad/plans/recipehub-build-plan.md is APPROVED — proceed with the 25-item work decomposition.
- Zoe's test strategy at .squad/plans/recipehub-test-strategy.md is APPROVED.
- Open-question resolutions (all per Mal's recommendation):
  1. Node.js version: **22 LTS**
  2. Aspire dashboard auth: **none** (dev-time only; Codespaces)
  3. Frontend package manager: **npm**
  4. Community Toolkit SQLite Aspire resource: **do NOT use** — file-based SQLite configured via ppsettings.json
- The 3 planted bugs (Cook Mode off-by-one, Search case sensitivity, Share token persistence) MUST be implemented at the locations Mal identified and MUST NOT be silently fixed. They are Challenge 05 material.
**Why:** User directive — explicit approval to start building.

### 2026-04-20T13:55:00Z: RecipeHub Build Plan — Aspire 13.2 Architecture
**By:** Mal (Lead / Architect)
**Status:** PROPOSED → APPROVED (2026-04-20T17:55:00Z)
**Plan File:** .squad/plans/recipehub-build-plan.md
**What:**
- Scaffold RecipeHub on .NET Aspire 13.2 with AppHost + ServiceDefaults pattern
- Api project: .NET 10 Minimal API, EF Core 10, SQLite (file-based, no Aspire resource)
- Web project: React 19 + TypeScript + Vite 6 via AddViteApp() from Aspire.Hosting.JavaScript
- API URL passed to frontend via VITE_API_BASE_URL environment variable
- Three planted bugs preserved in secondary features (Cook Mode, Search, Share) — marked DO NOT FIX
- Test projects with skipped bug-specific tests for Challenge 05 un-skipping
- 25 work items decomposed across Kaylee (backend), Inara (frontend), Zoe (tests), Mal (polish)
**Why:**
- Zack directive: use Aspire 13.2 as base template
- Aspire provides unified orchestration, dashboard, and service discovery for hackathon demos
- File-based SQLite is simplest for Codespaces; no container needed
**Open Questions (resolved):**
1. Node.js version? → 22 LTS
2. Aspire dashboard auth? → none for dev
3. Package manager? → npm
4. Use Community Toolkit SQLite integration? → no

### 2026-04-20: RecipeHub Test Strategy (v1)
**By:** Zoe (Tester/QA)
**Status:** PROPOSED → APPROVED (2026-04-20T17:55:00Z)
**What:**
Adopt a test strategy for RecipeHub v1 focused on:
1. **Integration-heavy pyramid:** 40 API integration tests, 25 frontend component tests, 15 unit tests, 0 E2E (deferred).
2. **SQLite in-memory database with shared connection** for backend tests (not EF Core InMemory provider).
3. **Planted bugs represented as skipped tests** with bug IDs in skip reason, describing expected-after-fix behavior.
4. **xUnit + WebApplicationFactory** for backend, **Vitest + React Testing Library + MSW** for frontend.
5. **Coverage target: 70%** lines, enforced in CI.

**Rationale:**
- **SQLite In-Memory Over EF InMemory:** EF Core InMemory provider does NOT enforce foreign key constraints, unique constraints, or SQL-specific behavior (e.g., LIKE operator semantics). SQLite in-memory with a shared open connection provides true parity with production while allowing test isolation via transactions.
- **Skip Planted Bug Tests:** The skipped test IS the acceptance criteria. Un-skipping the test after the fix is simpler than maintaining two tests per bug.
- **Defer Playwright E2E:** Integration tests via WebApplicationFactory already exercise full HTTP → EF Core → SQLite cycles. Frontend component tests with MSW cover user interactions.
- **70% Coverage Target:** RecipeHub is a hackathon demo, not production SaaS. 70% is achievable without testing pure UI presentational logic.

**Impact:**
- **Developers:** Clear test expectations before writing code. TDD-ready.
- **Coaches:** Skipped tests serve as Challenge 05 acceptance criteria — un-skip after bug fix.
- **Participants:** Tests demonstrate "what good looks like".
- **CI/CD:** Automated quality gate before merge.

### 2026-04-20: Item 1 — Aspire 13.2 solution scaffold complete
**By:** Kaylee (Backend)
**What:** Scaffolded the RecipeHub solution skeleton per Mal's build plan §2 and Zack's Aspire 13.2 directive. Build is green.
**Shape delivered:**
- RecipeHub.sln (classic .sln format)
- global.json (SDK 10.0.201, rollForward: latestFeature)
- .gitignore extended with Node + Vite entries
- src/RecipeHub.AppHost/ (minimal Aspire app)
- src/RecipeHub.ServiceDefaults/ (health/alive endpoints)
- src/RecipeHub.Api/ (minimal with DbContext registration)
- src/RecipeHub.Web/ (.gitkeep placeholder)
**Target framework:** net10.0
**Build:** dotnet build RecipeHub.sln — 0 warnings, 0 errors
**Ready to unblock:** Items 2, 3, 4, 20

### 2026-04-20: Item 2 — EF Core DbContext + Initial Migration
**By:** Kaylee (Backend)
**What:** Implemented EF Core data layer: 6 entity models (Recipe, RecipeStep, Tag, RecipeTag, ShareToken, Favorite) + RecipeDbContext with fluent configuration + InitialCreate migration.
**Key details:**
- Difficulty enum stored as **string** (TEXT) in SQLite
- UTC DateTime convention: explicit ValueConverter applied to every DateTime column (SQLite requires this)
- Composite PKs on RecipeTag; unique indexes on Tag.Name, ShareToken.Token, (RecipeStep.RecipeId, StepNumber), (Favorite.UserId, RecipeId)
- Cascade delete: Recipe → Steps/RecipeTags/ShareTokens/Favorites; Tag → RecipeTag
- .config/dotnet-tools.json pinned at repo root (dotnet-ef 10.0.0)
- Migration: Data/Migrations/20260420180648_InitialCreate.cs + Designer + ModelSnapshot
**Build:** dotnet build RecipeHub.sln — 0 warnings, 0 errors
**Deferred:** Seed data (Item 5), endpoints (Items 6/7/12/14/16/18), migrations at startup

### 2026-04-20: Item 3 — Vite + React 19 + TS scaffold in src/RecipeHub.Web/
**By:** Inara (Frontend Dev)
**What:** Hand-crafted Vite 6 + React 19 + TypeScript scaffold (not via 
pm create vite). All versions pinned exactly; TanStack Query provider already wired in main.tsx.
**Pinned versions (no caret):**
- react: 19.1.1
- react-dom: 19.1.1
- typescript: 5.7.2
- vite: 6.0.7
**Caret-ranged:**
- @vitejs/plugin-react: ^4.3.4
- @tanstack/react-query: ^5.62.7
- @types/react, @types/react-dom: ^19.0.2
- @types/node: ^22.10.2
- ESLint 9 flat-config stack
**vite.config.ts:**
- Reads process.env.PORT (Aspire injects it) → fallback 5173
- server.host: true (bind 0.0.0.0 for containers/Aspire)
- Dev-only /api proxy → VITE_API_BASE_URL
- Env typing: ImportMetaEnv augmented with VITE_API_BASE_URL: string
**Verification:**
- npm install → 184 packages, no critical errors
- npm run build → ✓ Vite 6.0.7, 77 modules, 661ms
- npm run lint → clean
**Node version note:** Local dev box has Node 24.11.0; plan target remains 22 LTS
**Deferred:** router, pages, components, hooks, API client (Items 9–17), Vitest (Item 21), AppHost wiring (Item 4 — still in flight)

### 2026-04-20: Item 4 — AppHost orchestration + Api dev CORS
**By:** Kaylee (Backend)
**What:** Wired Aspire 13.2 AppHost to orchestrate the Api project + Vite web app. Added development-only CORS policy to Api.
**AppHost changes:**
- Added ProjectReference to RecipeHub.Api (enables Projects.RecipeHub_Api code-gen)
- AppHost.cs: AddProject<Projects.RecipeHub_Api>("api"), AddViteApp("web", "../RecipeHub.Web"), WithReference, WithEnvironment VITE_API_BASE_URL
**Api changes:**
- Dev-only CORS policy (DevCorsPolicy): SetIsOriginAllowed(_ => true) + AllowAnyHeader + AllowAnyMethod + AllowCredentials
- Rationale: AllowAnyOrigin incompatible with AllowCredentials; predicate form allows any origin while reflecting it
- Applied only in IsDevelopment() (Production stays CORS-free)
**Coordination:**
- Item 2 (DbContext) landed in Program.cs concurrently; CORS inserted around existing AddDbContext call
- Item 3 (Web/.gitkeep) still in flight; AppHost references ../RecipeHub.Web which currently exists as .gitkeep
- Build still succeeds; Aspire resolves Vite path at runtime only
**Build:** dotnet build RecipeHub.sln — 0 warnings, 0 errors
**Deferred:** Production CORS tightening, endpoint additions (Items 6+), Web-side consumer (Inara)

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
