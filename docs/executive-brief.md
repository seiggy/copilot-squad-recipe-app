# RecipeHub Hackathon Demo Application - Executive Brief

**Prepared for:** Engineering Leadership / Hackathon Program Sponsors
**Date:** July 2025
**Classification:** Internal

---

## Business Challenge

Your organization has invested in GitHub Copilot licenses for every developer. Usage data tells a familiar story: a handful of enthusiasts use it constantly, while the majority treat it as an autocomplete tool or ignore it entirely. The licenses are paid for, but the productivity gains that justified the investment have not materialized at scale.

The root cause is not resistance. It is a lack of structured exposure. Developers who have never orchestrated an AI agent team, configured quality gates for AI-generated code, or debugged with an AI partner simply do not know what they are missing. They cannot adopt workflows they have never seen. Slide decks and documentation links do not change behavior. Hands-on practice does.

The question facing engineering leadership: how do you move hundreds of developers from "Copilot installed" to "Copilot integrated into daily work" - quickly, repeatably, and without pulling people away from delivery for more than half a day?

---

## Recommended Solution

We propose building **RecipeHub**, a purpose-built full-stack application that serves as the shared codebase for a 4-hour, hands-on hackathon program. RecipeHub is not a toy example. It is a realistic .NET 10 Minimal API + React 19 + TypeScript application backed by SQLite, pre-loaded with seed data and running entirely on each participant's machine via GitHub Codespaces. Features include recipe CRUD, a Cook Mode step-by-step walkthrough, search and filtering, recipe sharing via token links, and a Favorites feature left as a stub for participants to build during the hackathon.

Critically, three bugs are planted in secondary features - a Cook Mode off-by-one error that skips steps, a case-sensitive search defect that returns zero results for mixed-case queries, and a share token persistence bug where tokens are generated after the database save so all share links return 404. These bugs create realistic debugging exercises that demonstrate Copilot's diagnostic capabilities on real production-style code.

### The Seven Challenges

The hackathon is structured as seven progressive challenges that take participants from first setup through autonomous AI operations:

- **Ch00 - Base Camp (20 min):** Environment setup and tool verification. Participants open a Codespace, run the app, confirm their toolchain works.

- **Ch01 - Assemble Your Squad (25 min):** Initialize GitHub Copilot Squad, configure an AI agent team (backend agent, frontend agent, tester agent), and set up routing rules that direct tasks to the right agent.

- **Ch02 - Ship a Feature (35 min):** Build the Favorites feature end-to-end using multi-agent delegation. Participants experience Copilot agents scaffolding across API, database, and frontend layers simultaneously.

- **Ch03 - Quality Gates (30 min):** Set up Git hooks via Husky, configure Copilot hooks (preToolUse), and enforce linter rules so AI-generated code must pass automated checks before it lands.

- **Ch04 - Test Coverage Blitz (30 min):** Use the Tester agent to generate tests targeting 80%+ code coverage. Participants create a SKILL.md file that teaches the agent project-specific testing conventions.

- **Ch05 - Break-Fix (35 min):** Diagnose and fix the three planted bugs using Copilot agents. This is where participants experience AI-assisted debugging on code they did not write - the closest simulation of real-world maintenance work.

- **Ch06 - Autonomous Operations (35 min):** Configure Ralph watch mode for continuous autonomous operation, add governance hooks that enforce human approval at key checkpoints, and wire up .NET Aspire observability to monitor what the agents are doing.

Each challenge builds on the previous one. Coaches circulate to keep participants on track and surface insights.

---

## Expected Business Impact

### Developer Productivity

Industry benchmarks consistently show 30-55% productivity gains for developers who actively use AI coding assistants. GitHub's own research reports 55% faster task completion. A 4-hour hackathon compresses weeks of organic self-discovery into a single focused session, accelerating the point at which each participant begins realizing those gains in daily work.

For a team of 40 developers, even a conservative 20% productivity improvement in the first month post-hackathon translates to meaningful capacity recovery - equivalent to gaining 8 additional developer-days per month.

### Training Efficiency

The RecipeHub application is built once and reused indefinitely across unlimited cohorts. Unlike workshop content that goes stale, the application serves as a durable training asset. The one-time development investment of approximately $8,400 amortizes across every subsequent event, driving the marginal cost per cohort toward zero.

### Cost Per Trained Developer

For the first cohort of 40 participants, the fully loaded cost per developer (including app development, Codespaces compute, and coach time) is approximately $210. For every subsequent cohort, the marginal cost drops to Codespaces compute only - under $2 per developer - since the application already exists and coaches improve with repetition.

### Time to Proficiency

Self-directed Copilot adoption typically takes 4-6 weeks before a developer integrates AI assistance into routine workflow. The structured hackathon format compresses this to 4 focused hours, with participants leaving the session having completed real coding tasks using every major Copilot capability: multi-agent orchestration, quality gates, test generation, AI-assisted debugging, and autonomous operations with governance guardrails.

### Risk Reduction

Challenges 03 through 06 explicitly teach governance patterns - quality gates via Husky and Copilot hooks, linter enforcement on AI-generated code, and human-in-the-loop checkpoints for autonomous operations. Developers who learn these patterns during the hackathon are far less likely to merge AI-generated code without appropriate review, reducing the risk of defects reaching production.

---

## Timeline

| Phase | Duration | Activities |
|-------|----------|------------|
| **Phase 1: Foundation** | Days 1-2 | .NET Minimal API with recipe CRUD, SQLite database, EF Core models, seed data (12 recipes, 10 tags, ~70 steps) |
| **Phase 2: Frontend Core** | Days 3-5 | React 19 SPA with all pages (Home, Recipe List, Recipe Detail, Recipe Edit, Favorites stub), API integration via TanStack Query, Vite proxy configuration |
| **Phase 3: Secondary Features and Bugs** | Days 6-8 | Cook Mode step-by-step walkthrough, search and filtering, share-via-token links - each with its planted bug wired in naturally |
| **Phase 4: Polish and Packaging** | Days 9-10.5 | Dev container configuration, test infrastructure, challenge file scaffolding, README, coach guide, dry-run verification |

**Total development effort:** 10.5 person-days

**First hackathon ready:** approximately 3 weeks from kickoff (assuming one dedicated full-stack developer)

**Subsequent events:** near-zero preparation time - clone the template repository, provision Codespaces, and run

---

## Investment Summary

| Category | Type | Estimated Cost |
|----------|------|----------------|
| Application development (senior full-stack developer, 10.5 days at $800/day) | One-time | $8,400 |
| GitHub Codespaces compute per event (40 participants, 4-core machines, 5 hours) | Per event | ~$72 |
| GitHub Copilot licenses | Per event | Already budgeted (org-wide rollout) |
| Coach preparation and facilitation (2-4 coaches, 8 hours each) | Per event | 16-32 person-hours (internal staff) |

**Break-even analysis:** If the hackathon accelerates meaningful Copilot adoption by even one week for 40 developers, the recovered productivity (conservatively valued at $300/developer/week) exceeds the total program cost in the first month.

---

## Why This Approach Works

RecipeHub is not a contrived tutorial. It is a realistic codebase with real bugs, incomplete features, and the kind of multi-layer complexity (API, database, frontend, routing, state management) that developers encounter every day. When participants fix the share token persistence bug or wire up the Favorites feature using Copilot agents, they are practicing the exact workflows they will use on Monday morning with their own production code.

The hackathon format creates social proof across the engineering organization. Forty developers who have personally experienced AI-assisted debugging and multi-agent feature development become advocates who pull their teammates forward - a force multiplier that no amount of documentation can replicate.

---

## Next Steps

1. **Approve RecipeHub application development.** Confirm budget allocation for approximately 10.5 days of developer time ($8,400).

2. **Assign a full-stack developer.** Identify one engineer comfortable with .NET and React to build the application. Familiarity with GitHub Codespaces is helpful but not required.

3. **Schedule the dry-run hackathon.** Block a half-day session 3 weeks from kickoff for 4-6 internal volunteers to walk through all seven challenges and surface friction.

4. **Identify the first cohort.** Select 20-40 developers for the inaugural event. Prioritize teams with the lowest current Copilot usage rates for maximum impact.

5. **Book coaches.** Recruit 1 coach per 8-10 participants from senior engineering staff or the developer advocacy team. Coaches need the facilitation guide and one hour of preparation.

---

*This document is intended for engineering leadership reviewing the hackathon program investment. For technical architecture details, refer to the Solution Design document. For detailed cost modeling, refer to the Cost Estimation document. For delivery phasing and milestones, refer to the Delivery Plan.*
