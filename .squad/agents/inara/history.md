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

