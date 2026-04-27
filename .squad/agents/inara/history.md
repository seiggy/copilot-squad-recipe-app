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

### 2026-04-20 — Item 3: Vite + React 19 + TS scaffold

**Exact pinned versions (package.json, no caret):**
- `react`: `19.1.1`
- `react-dom`: `19.1.1`
- `typescript`: `5.7.2`
- `vite`: `6.0.7` (explicitly stayed on 6.x; did NOT jump to 7)

**Caret-ranged (deliberately floats within major):**
- `@tanstack/react-query`: `^5.62.7` (resolved to 5.99.0)
- `@vitejs/plugin-react`: `^4.3.4` (resolved to 4.7.0, Vite 6 compatible)
- `@types/react` / `@types/react-dom`: `^19.0.2`
- `@types/node`: `^22.10.2` (needed by `vite.config.ts` for `process.env.PORT`)
- ESLint 9 flat-config + `typescript-eslint` 8

**Vite config rationale (`vite.config.ts`):**
- `server.port` reads `process.env.PORT` (Aspire 13.2's `AddViteApp()` injects it) with a `5173` fallback for standalone runs.
- `server.host: true` binds 0.0.0.0 so Aspire/containers can reach the dev server.
- `server.proxy['/api']` targets `VITE_API_BASE_URL || http://localhost:5000` — this path is ONLY exercised when running `npm run dev` outside Aspire. Under Aspire the app reads `import.meta.env.VITE_API_BASE_URL` directly and hits the Api endpoint; the proxy is effectively dead code in that mode.

**TypeScript env typing:**
- `src/vite-env.d.ts` references `vite/client` and augments `ImportMetaEnv` with `VITE_API_BASE_URL: string` so `import.meta.env.VITE_API_BASE_URL` is typed everywhere — no `any`, no string-indexing.

**Scaffold approach:** Went with approach B (hand-crafted files) rather than `npm create vite@latest` to pin exact versions up front without a second edit pass. Template shape matches the official `react-ts` template (separate `tsconfig.app.json` / `tsconfig.node.json` with project references).

**Node version note:** Local machine has Node 24.11.0, not 22 LTS. Proceeded anyway — build + lint both pass. CI / Codespaces should still target 22 LTS per the plan.

**Gotchas:**
- Had to add `@types/node` — `vite.config.ts` uses `process.env` and `tsconfig.node.json` sets `"types": ["node"]`.
- Dropped `erasableSyntaxOnly` compiler option — TS 5.8+ only, we're on 5.7.2.
- `main.tsx` wraps `<App />` in `<QueryClientProvider>` with a default `QueryClient`. No queries yet — that's Item 8 territory.
- `App.tsx` renders a title + a debug line showing `VITE_API_BASE_URL ?? '(not set)'`. Zero page/component scaffolding beyond that — Items 9+ own the real UI.

**Verification:**
- `npm run build` → vite 6.0.7, 77 modules transformed, ✓ built in 661ms.
- `npm run lint` → clean, no warnings.


### 2026-04-20 — Item 9: Base UI components

Shipped Button / Card / Spinner / Badge in `src/components/ui/` with co-located CSS Modules + `index.ts` barrel. Button: 4 variants × 3 sizes + `loading`; Card: clickable variant is a proper `role=button` with keyboard handler; Spinner: `role=status` + sr-only label; Badge: 4 semantic variants. No new deps, vanilla CSS only. `npm run build` and `npm run lint` both clean. `App.tsx` untouched — wiring is a later item.

### 2026-04-20 — Item 8: Typed API client module

Shipped `src/api/{types,client,index}.ts` in `src/RecipeHub.Web`. Native fetch, zero new deps. camelCase types (server uses System.Text.Json defaults — confirmed no naming policy in Program.cs). `apiClient` covers the 6 existing endpoints (list/get/create/update/delete recipes + list tags); search/cook-mode/share/favorites deferred to their owning items. `ApiError` class surfaces `{status, message, body}`, pulling `title` off ProblemDetails. 204 returns `undefined`. `credentials: 'include'` matches dev CORS. `VITE_API_BASE_URL` required in prod, dev falls back to http://localhost:5000 with a warn. `npm run build` + `npm run lint` both clean.

### 2026-04-20 — Items 10+11+19: Pages, Routing, Query wiring

Shipped 5 pages (Home/List/Detail/Edit/Favorites) + 6 TanStack Query hooks + react-router-dom v7.14.1 wiring. Dual-mode Edit page (create via /recipes/new, edit via /recipes/:id/edit) with native useState form, dynamic step reorder, tag checkbox multi-select. Trivial title filter on list page marked TODO for Item 15's SearchBar. Cook Mode button links to /recipes/:id/cook (route owned by Item 13). No apiClient changes — Item 8's 6 endpoints cover full CRUD. CSS Modules throughout. `npm run build` (118 modules, 1.06s) + `npm run lint` both clean.

### 2026-04-20 — HTTP 431 fix: raised Node max-http-header-size to 32KB in Web dev script (package.json "dev" → `node --max-http-header-size=32768 node_modules/vite/bin/vite.js`). Root cause: Aspire-injected env/headers blew past Node's 8KB default. vite.config.ts already correct (host:true, PORT honored) — no change. Build + lint clean.

### 2026-04-20 — Items 13+15+17: Cook Mode, Search, Share UIs

Shipped useCookMode/useTimer/CookModePage (route /recipes/:id/cook), useSearch + SearchBar (300ms internal debounce) + FilterPanel wired into RecipeListPage, ShareButton on RecipeDetailPage, SharedRecipePage (route /shared/:token) with bare layout via useLocation check in App.tsx. apiClient +4 methods (getCookStep, searchRecipes, shareRecipe, getSharedRecipe), +2 types (CookModeDto, ShareDto). BUG 1 frontend planted at useCookMode.ts L19 (useState(1)) — no comment. Bugs 2/3 untouched; share page correctly lands on 404 branch end-to-end as expected. No new deps, vanilla CSS modules. npm run build (133 modules, 865ms) + npm run lint both clean.
