# RecipeHub Hackathon App - Cost Estimation

## 1. Cost Overview

RecipeHub is a local-only application. It runs on each participant's machine (or in a GitHub Codespace) using .NET 10, React 19, and SQLite. There are no Azure service costs, no cloud hosting fees, and no production infrastructure to budget for.

The costs for this project fall into three categories:

1. **Development tooling and licenses** - GitHub Copilot subscriptions and free SDKs.
2. **Hackathon delivery infrastructure** - GitHub Codespaces compute for each event.
3. **Starter app development** - One-time engineering effort to build the template repository.

---

## 2. Development Tooling Costs

| Item | Cost | Notes |
|------|------|-------|
| GitHub Copilot Business | $19/user/month | Required for all hackathon participants |
| GitHub Copilot Enterprise | $39/user/month | Alternative if org already has Enterprise tier |
| Visual Studio 2026 | Included with VS subscription or Community (free) | Community edition is sufficient for the hackathon |
| VS Code | Free | Recommended editor; fully supported by dev container config |
| .NET 10 SDK | Free | Installed automatically in the dev container |
| Node.js 20 LTS | Free | Installed automatically in the dev container |
| SQLite | Free | Embedded database; no server process required |
| Squad CLI | Free (open source) | Copilot Squad orchestration tooling |

Copilot licensing is the only recurring software cost. Most organizations running a Copilot hackathon will already have licenses provisioned for participants. If licenses need to be added for the event, the lead time is minimal - Copilot Business licenses can be assigned in the GitHub organization settings and are active immediately.

---

## 3. Hackathon Delivery Costs (GitHub Codespaces)

GitHub Codespaces is the recommended development environment. It eliminates "works on my machine" issues and ensures every participant starts with an identical, fully configured workspace.

### Per-Event Compute Estimate

| Parameter | Value |
|-----------|-------|
| Participants | 20-40 |
| Codespace machine type | 4-core, 16 GB RAM, 32 GB storage |
| Active hackathon duration | 4 hours |
| Setup and teardown buffer | 1 hour |
| Total billable time per participant | 5 hours |
| Codespace rate (4-core) | $0.36/hour |
| Cost per participant | $1.80 |
| **Total per event (20 participants)** | **~$36** |
| **Total per event (40 participants)** | **~$72** |

### Idle Codespace Costs

GitHub Codespaces auto-stops after 30 minutes of inactivity by default. If a participant walks away during lunch or a break, the Codespace pauses and stops incurring compute charges. Storage charges continue at $0.07/GB/month, but for a 4-hour event this is negligible (under $0.01 per participant).

Worst-case idle scenario: if 10 participants leave Codespaces running overnight after the event, the additional cost is approximately $0.36/hour x 10 x 8 hours = $28.80. Setting an organization-level idle timeout policy of 30 minutes prevents this entirely.

### Prebuild Costs (Optional)

Prebuilds create a ready-to-use Codespace image so participants skip the initial container build step (which otherwise takes 2-3 minutes). Prebuild compute runs once per commit to the template branch.

| Prebuild Parameter | Value |
|--------------------|-------|
| Prebuild machine type | 4-core |
| Prebuild duration per run | ~5 minutes |
| Prebuild frequency | 1-2 times before each event |
| Cost per prebuild run | ~$0.03 |
| Monthly storage for prebuild image | ~$2-3/month |
| **Recommended monthly budget for prebuilds** | **~$10/month** |

Prebuilds are recommended for events with 20+ participants. The 2-3 minute savings per participant adds up to over an hour of collective wait time eliminated.

---

## 4. Starter App Development Cost Estimate

The following table estimates the one-time engineering effort to build the RecipeHub starter app from the software requirements document.

| Component | Effort (person-days) | Description |
|-----------|---------------------|-------------|
| .NET API - recipe CRUD | 2.0 | Recipe and Tag models, EF Core DbContext with SQLite, REST endpoints for list/detail/create/update/delete, Swagger configuration |
| .NET API - secondary features | 1.5 | Cook Mode session endpoints, full-text search, share link generation; includes planting 3 deliberate bugs across these features |
| React frontend - core pages | 3.0 | Home/browse page, recipe detail page, create/edit form, tag filtering, responsive layout, Axios API client, React Router configuration |
| React frontend - Cook Mode UI | 1.0 | Step-by-step display, built-in timer component, previous/next navigation, progress indicator |
| Dev container setup | 0.5 | Multi-stage Dockerfile, devcontainer.json with extensions and port forwarding, post-create scripts for dependency installation |
| Testing infrastructure | 0.5 | xUnit project scaffolding for .NET, Vitest configuration for React; test files are empty stubs that participants fill in during the hackathon |
| Seed data | 0.5 | 12 complete recipes across multiple cuisines, each with 5-10 preparation steps, associated tags, and realistic metadata |
| Bug planting and verification | 1.0 | Implement 3 distinct bugs (off-by-one in steps, missing null check, incorrect search filter); verify each bug does not block Chapters 00-04; document expected vs. actual behavior for coaching guide |
| Documentation and README | 0.5 | Getting started guide, API endpoint reference, dev container usage instructions, chapter overview for participants |
| **Total** | **10.5 person-days** | |

### Cost Calculation

| Rate Model | Calculation | Total |
|------------|-------------|-------|
| Average developer rate ($800/day) | 10.5 days x $800 | **$8,400** |
| Senior developer rate ($1,200/day) | 10.5 days x $1,200 | $12,600 |
| Internal FTE (fully loaded, estimated) | 10.5 days x $600 | $6,300 |

The $800/day rate is used as the baseline estimate throughout this document. Actual cost depends on whether the work is done by internal staff or contractors.

---

## 5. ROI Framing

### Cost Per Trained Developer

The starter app is built once and reused across multiple hackathon events.

| Metric | First Event (40 ppl) | After 3 Events (120 ppl) | After 10 Events (400 ppl) |
|--------|----------------------|--------------------------|---------------------------|
| Development cost (amortized) | $210/developer | $70/developer | $21/developer |
| Codespaces cost per developer | $1.80 | $1.80 | $1.80 |
| Copilot license cost (already provisioned) | $0 incremental | $0 incremental | $0 incremental |
| **Effective cost per developer** | **~$212** | **~$72** | **~$23** |

### Productivity Gains from Copilot Proficiency

Industry data from GitHub's own research and third-party studies consistently shows measurable productivity improvements from Copilot adoption:

- **30-55% faster task completion** for code generation tasks (GitHub 2023 developer survey).
- **1-2 hours saved per developer per week** on routine coding, test writing, and documentation tasks once proficiency is established.
- **Higher code review throughput** as Copilot-assisted code tends to follow more consistent patterns.

Applying conservative estimates to a single hackathon cohort:

| Scenario | Weekly Hours Saved | Annual Hours Saved | Value at $100/hr |
|----------|-------------------|-------------------|-------------------|
| 20 developers saving 1 hr/week | 20 hrs/week | 1,040 hrs/year | $104,000/year |
| 40 developers saving 1 hr/week | 40 hrs/week | 2,080 hrs/year | $208,000/year |
| 40 developers saving 2 hrs/week | 80 hrs/week | 4,160 hrs/year | $416,000/year |

Against a one-time development cost of $8,400 and per-event costs under $100, the payback period is measured in days, not months.

### Copilot License ROI

The hackathon accelerates time-to-proficiency with Copilot. Organizations that invest in Copilot licenses without structured training see slower adoption curves and lower utilization. A hands-on hackathon with a realistic codebase compresses the learning curve from weeks of organic discovery into a single focused session.

---

## 6. Optimization Recommendations

**Reduce participant wait time with prebuilds.** Enable Codespaces prebuilds on the template repository. This adds roughly $10/month in compute and storage but eliminates the 2-3 minute container build that every participant otherwise hits at the start of the event. For a 40-person event, that is over an hour of collective wait time removed.

**Enforce idle timeout policies.** Set the organization-level Codespace idle timeout to 30 minutes. This is the default, but verify it has not been overridden. This single setting prevents runaway compute costs from forgotten Codespaces.

**Reuse the template repository across events.** The RecipeHub repo is designed as a GitHub template repository. Each event creates participant copies via "Use this template" or Codespace deep links. No per-event repository maintenance is needed beyond verifying the dev container builds cleanly.

**Right-size Copilot licensing for the program.** If participants do not already have Copilot licenses, provision Copilot Business ($19/user/month) for the duration of the hackathon program. Licenses can be assigned and removed on a monthly basis. If the organization already has Copilot Enterprise ($39/user/month), use those existing licenses rather than provisioning a separate tier.

**Budget for coaching staff.** Plan for 1 coach per 8-10 participants. Coaches do not need separate Codespaces; they pair with participants on their machines. For a 40-person event, budget 4-5 coaches for 4 hours each (16-20 person-hours of coaching time).

**Delete Codespaces after the event.** Organization admins can bulk-delete Codespaces after the hackathon ends. This stops all storage charges immediately and keeps the organization's Codespace inventory clean.

---

## 7. Total Cost Summary

| Category | One-Time | Per Event (20 ppl) | Per Event (40 ppl) |
|----------|----------|---------------------|---------------------|
| Starter app development | $8,400 | -- | -- |
| Copilot licenses (monthly) | -- | Already budgeted | Already budgeted |
| Codespaces compute (active) | -- | ~$36 | ~$72 |
| Codespaces prebuilds (monthly) | -- | ~$10/month | ~$10/month |
| Coach time (4 hrs active) | -- | 8-12 person-hours (2-3 coaches) | 16-20 person-hours (4-5 coaches) |
| Venue and logistics | -- | Varies by location | Varies by location |
| **Total hard costs** | **$8,400** | **~$46** | **~$82** |

Hard costs exclude coach time (internal staff) and venue logistics (organization-dependent). The per-event compute cost is intentionally low - this is a local-only app with no cloud services to provision or manage.

For organizations running quarterly hackathon events over a year (4 events, 40 participants each, 160 total developers trained), the all-in hard cost is approximately $8,400 + ($82 x 4) = **$8,728** - or roughly **$55 per developer trained**.
