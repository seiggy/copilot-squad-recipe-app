# RecipeHub Dev Container

One-click development environment for the RecipeHub hackathon starter.

**What this provides:**

- .NET 10 SDK (base image `mcr.microsoft.com/devcontainers/dotnet:1-10.0`) — matches `global.json` (10.0.201, rollForward `latestFeature`).
- Node.js **22 LTS** + npm for the Vite 6 / React 19 frontend (`src/RecipeHub.Web`).
- GitHub CLI (`gh`) and PowerShell for hackathon-friendly tooling.
- VS Code extensions pre-installed: C# Dev Kit, ESLint, Prettier, Tailwind IntelliSense, GitHub Copilot + Copilot Chat.
- `postCreateCommand` runs `dotnet restore RecipeHub.sln` and `npm install` in the web project — you're ready to `dotnet run --project src/RecipeHub.AppHost` as soon as it finishes.

**Forwarded ports:** 5000/5001 (Api http/https) and 17050 (Aspire dashboard typical range). Aspire will announce actual dynamic ports in its console output on startup — use those if different.

**Note on Node:** Local dev machines may have Node 24 installed (see Item 3 scaffold); the container pins to Node 22 LTS per the build plan directive. Lockfiles must resolve on both.
