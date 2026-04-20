# RecipeHub — Hackathon Starter Application

This project contains the architecture and requirements documentation for **RecipeHub**, a .NET 10 + React 19 recipe-sharing application used as the demo project in the [GitHub Copilot & Squad Developer Workflow Hackathon](../../hackathons/copilot-squad-dev-workflow/).

## What Is This?

RecipeHub is the app that hackathon participants work with throughout 7 progressive challenges. It is a fully functional recipe management app with three purposeful bugs planted in secondary features — these bugs become the focus of Challenge 05 (Break-Fix).

## Documentation

| Document | Description |
|----------|-------------|
| [Executive Brief](docs/executive-brief.md) | Stakeholder summary — business case, ROI, and next steps |
| [Solution Design](docs/solution-design.md) | Full SRD — features, API endpoints, data models, bug specifications |
| [Architecture Diagram](docs/architecture-diagram.drawio) | Visual architecture (open in draw.io or VS Code drawio extension) |
| [Data Assessment](docs/data-assessment.md) | SQLite schema, seed data, data quality rules |
| [Responsible AI](docs/responsible-ai.md) | RAI assessment for AI-assisted development workflows |
| [Cost Estimation](docs/cost-estimation.md) | Development and hackathon delivery costs |
| [Delivery Plan](docs/delivery-plan.md) | Phased build plan with milestones and risks |

## Tech Stack

- **Backend:** .NET 10 Minimal API + EF Core 10 + SQLite
- **Frontend:** React 19 + TypeScript + Vite 6 + TanStack Query v5
- **Environment:** GitHub Codespaces (recommended) or local dev container

## Planted Bugs (Challenge 05)

The starter app ships with 3 dormant bugs in secondary features. See [Solution Design §8](docs/solution-design.md#8-purposeful-bugs--challenge-05-specification) for full details:

1. **Cook Mode Off-By-One** — First step skipped, last step unreachable
2. **Search Case Sensitivity** — Multi-word and mixed-case queries fail
3. **Share Token Persistence** — Generated tokens never saved to database, links always 404

These bugs do not block Challenges 00–04 (recipe CRUD, favorites, quality gates, tests).

## Related Hackathon

The hackathon materials live at [`outputs/hackathons/copilot-squad-dev-workflow/`](../../hackathons/copilot-squad-dev-workflow/). Challenge 05 should be updated to reference the new bug descriptions from this SRD.
