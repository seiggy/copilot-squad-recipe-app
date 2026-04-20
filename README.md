# RecipeHub

A full-stack recipe management application built with .NET Aspire 13.2, demonstrating modern cloud-native patterns for the **GitHub Copilot & Squad Developer Workflow Hackathon**. Participants learn to leverage AI agents for feature development, testing, and debugging through seven progressive challenges.

## Architecture

RecipeHub uses **.NET Aspire 13.2** for orchestration, combining a **.NET 10 Minimal API** backend with **EF Core 10** and **SQLite** for data persistence. The frontend is a **React 19** SPA built with **TypeScript**, **Vite 6**, and **TanStack Query v5** for server state management. Aspire's dashboard provides unified observability across all services during development.

## Quickstart

### Prerequisites   

- .NET SDK 10.0.201 or later
- Node.js 22+ (LTS recommended)
- Git

### Option A: GitHub Codespaces (Recommended)

Click the button below to launch a pre-configured Codespace:

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/seiggy/copilot-squad-recipe-app)

The devcontainer installs all dependencies automatically. Once ready, run:

```bash
dotnet run --project src/RecipeHub.AppHost
```

### Option B: Local Development

```bash
# Clone the repository
git clone https://github.com/seiggy/copilot-squad-recipe-app.git
cd copilot-squad-recipe-app

# Restore and run
dotnet run --project src/RecipeHub.AppHost
```

### What to Expect  

When the AppHost starts, you'll see:

| Service | URL | Description |
|---------|-----|-------------|
| Aspire Dashboard | http://localhost:17050 | Service orchestration & telemetry |
| RecipeHub API | http://localhost:5000 | .NET Minimal API endpoints |
| RecipeHub Web | http://localhost:5173 | React frontend |

The SQLite database is created automatically with 12 seed recipes on first run.

## Project Structure

```
copilot-squad-recipe-app/
├── src/
│   ├── RecipeHub.AppHost/         # Aspire orchestration (entry point)
│   ├── RecipeHub.ServiceDefaults/ # Shared Aspire configuration
│   ├── RecipeHub.Api/             # .NET 10 Minimal API + EF Core
│   │   ├── Data/                  # DbContext, migrations, seed data
│   │   ├── Models/                # Entity classes
│   │   ├── Endpoints/             # Route handlers
│   │   └── Dtos/                  # Request/response models
│   └── RecipeHub.Web/             # React 19 + Vite 6 frontend
│       └── src/
│           ├── api/               # Typed API client
│           ├── components/        # UI components
│           ├── hooks/             # TanStack Query hooks
│           └── pages/             # Route pages
├── tests/
│   └── RecipeHub.Api.Tests/       # xUnit integration tests
├── docs/                          # Architecture & planning docs
└── .devcontainer/                 # Codespaces configuration
```

## Running Tests

### Backend Tests

```bash
dotnet test
```

### Frontend Tests

```bash
cd src/RecipeHub.Web
npm install
npm run test
```

### Linting

```bash
# Frontend
cd src/RecipeHub.Web
npm run lint

# Backend (optional - uses .editorconfig)
dotnet format --verify-no-changes
```

## Hackathon Challenges

RecipeHub is designed for a 4-hour hackathon with seven progressive challenges:

| Challenge | Topic | Duration |
|-----------|-------|----------|
| Ch00 | Base Camp — Environment setup | 20 min |
| Ch01 | Assemble Your Squad — Configure AI agents | 25 min |
| Ch02 | Ship a Feature — Build Favorites with agents | 35 min |
| Ch03 | Quality Gates — Linting & formatting enforcement | 30 min |
| Ch04 | Test Coverage Blitz — Agent-generated tests | 30 min |
| Ch05 | Break-Fix — Debug planted bugs | 35 min |
| Ch06 | Autonomous Operations — Governance & monitoring | 35 min |

See the [`docs/`](docs/) folder for detailed architecture documentation.

### Challenge 05: Known Issues

The starter app intentionally includes three bugs in secondary features for the Break-Fix challenge:

1. **Cook Mode Navigation** — Step navigation doesn't reach all steps correctly
2. **Recipe Search** — Some search queries return unexpected results
3. **Share Links** — Generated share links may not work as expected

These issues don't affect the core recipe CRUD workflows used in Challenges 00–04.

## Documentation

| Document | Description |
|----------|-------------|
| [Solution Design](docs/solution-design.md) | API endpoints, data models, feature specs |
| [Executive Brief](docs/executive-brief.md) | Business case and ROI analysis |
| [Data Assessment](docs/data-assessment.md) | Database schema and seed data |
| [Responsible AI](docs/responsible-ai.md) | RAI assessment for AI workflows |

## Contributing

This is a hackathon starter template. For issues or improvements:

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

MIT License — see [LICENSE](LICENSE) for details.
