# Current Focus

**As of:** 2026-04-20

## ✓ Items 1–4 Complete

- **Item 1:** Aspire 13.2 solution scaffold (global.json, sln, AppHost/ServiceDefaults/Api, Web/.gitkeep)
- **Item 2:** EF Core data layer (6 models, DbContext, InitialCreate migration, .config/dotnet-tools.json)
- **Item 3:** Vite 6 + React 19 + TS scaffold (exact pinned versions, TanStack Query provider wired)
- **Item 4:** AppHost orchestration + Api dev CORS

All builds green. All decisions merged into `.squad/decisions.md`.

## → Items 5–9 Ready to Start

**Batch: Seed data, Recipe CRUD, Tag endpoints, Vite UI components**

- Item 5 (Kaylee): Seed data — Recipe, Tag, ShareToken samples
- Item 6 (Kaylee): Recipe CRUD endpoints (GET all, GET {id}, POST, PUT, DELETE)
- Item 7 (Kaylee): Tag endpoints (GET all, POST, DELETE)
- Item 8 (Inara): TanStack Query hooks wiring (useRecipes, useRecipeDetail, useCreateRecipe, etc.)
- Item 9 (Inara): Vite routing + page layout (RecipeList, RecipeDetail, RecipeForm)

**Stack reference:** .NET 10 + Aspire 13.2, EF Core 10 + SQLite (file-based), React 19 + TypeScript + Vite 6 + TanStack Query v5 | Node 22 LTS | npm
**Planted bugs:** Cook Mode off-by-one, Search case sensitivity, Share token persistence — implement exactly, do NOT fix (Challenge 05)
**Test strategy:** xUnit + Vitest/RTL, SQLite in-memory, skipped bug tests, 70% coverage target (E2E deferred)
