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

### 2026-04-20: Aspire 13.2 Build Plan Created

**Plan file:** `.squad/plans/recipehub-build-plan.md`

**Key architectural decisions:**

1. **Aspire JavaScript Integration** — Use `Aspire.Hosting.JavaScript` (13.2.2) with `AddViteApp()` for React frontend. Critical: do NOT call `.WithHttpEndpoint()` on Vite resources; `AddViteApp` auto-registers one. Duplicate endpoint calls cause runtime errors.

2. **API URL Discovery** — Vite only exposes `VITE_`-prefixed env vars to client code. Pass API URL via `WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"))`. Frontend reads `import.meta.env.VITE_API_BASE_URL`.

3. **SQLite: No Aspire Resource Needed** — The Community Toolkit `CommunityToolkit.Aspire.Hosting.Sqlite` exists but adds orchestration complexity for a file-based DB. Simpler to configure connection string in `appsettings.json` directly. Zero benefit for local-only hackathon app.

4. **Planted Bug Locations (DO NOT FIX):**
   - BUG-001: `CookModeEndpoints.cs` (stepNumber - 1) + `useCookMode.ts` (useState(1))
   - BUG-002: `SearchEndpoints.cs` (string.Contains is case-sensitive in SQLite)
   - BUG-003: `ShareEndpoints.cs` (Token assigned after SaveChangesAsync)

5. **Test Strategy** — Skipped test files with explicit bug IDs (`[Fact(Skip = "BUG-001...")]` / `describe.skip`) so Challenge 05 participants un-skip them.

6. **Work parallelization** — 25 work items; items 2-4 can run in parallel after scaffold; frontend/backend work largely parallelizable after API contracts defined.

### 2026-04-20: Items 24–25 Complete — Build Finalized

Completed final polish: rewrote README.md (149 lines), planted 3 formatting inconsistencies (README trailing whitespace + TestBase.cs K&R braces), created `docs/bug-reproduction.md` coach reference, verified all builds/tests green. 25/25 items done. RecipeHub ready for hackathon.
