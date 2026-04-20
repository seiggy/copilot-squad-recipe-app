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
