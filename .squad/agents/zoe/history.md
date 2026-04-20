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

### 2026-04-20: Test Strategy for RecipeHub v1

**Database choice:** SQLite in-memory with shared open connection beats EF Core InMemory provider — foreign key constraints and transaction behavior match production SQLite file database. In-memory provider silently ignores constraint violations, creating false negatives in tests.

**Planted bug test pattern:** SKIP tests representing expected-after-fix behavior, with bug ID in skip reason. When bug is fixed, un-skip the test — the test IS the acceptance criteria. Avoids duplication of "broken behavior test" vs "fixed behavior test."

**Test pyramid for data-driven apps:** Heavy integration (API + DB), moderate component (stateful UI), minimal unit (little extractable logic in Minimal API). ~80 tests targeting 70% coverage is realistic for v1.

**Planted bugs as acceptance criteria:** Each bug gets at least two skipped tests (backend + frontend) describing correct behavior. SRD §8 bug specs are detailed enough to write assertions before implementation exists.

**Case sensitivity in SQLite:** `string.Contains()` → `instr()` (case-sensitive). `EF.Functions.Like()` → `LIKE` (case-insensitive for ASCII). Critical for search bug diagnosis.

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- 2026-04-20 — Item 20: scaffolded 	ests/RecipeHub.Api.Tests/ (xunit.v3 1.0.1, Test SDK 17.12.0, Mvc.Testing + EF.Sqlite 10.0.0), added to sln, placeholder skipped test green. Build 0/0, test 0 failed / 1 skipped.
