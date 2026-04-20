# Data Assessment - RecipeHub Recipe-Sharing Application

**Application:** RecipeHub (Copilot Squad hackathon demo)
**Stack:** .NET 10 + React 19 + SQLite
**Deployment:** Local-only, single machine, no cloud services
**Date:** 2025-01-15

---

## 1. Data Sources Overview

RecipeHub uses a single SQLite database file (`recipes.db`) as its sole data store. There are no external APIs, no cloud databases, no message queues, and no third-party data feeds. The entire dataset is self-contained within the SQLite file, which is created and seeded automatically by Entity Framework Core migrations on first application startup.

| Property | Value |
|---|---|
| Database engine | SQLite 3 (via `Microsoft.Data.Sqlite`) |
| Database file | `api/recipes.db` |
| ORM | Entity Framework Core 10 |
| External data sources | None |
| Cloud dependencies | None |
| Network requirements | None (fully offline capable) |

---

## 2. Database Schema

### 2.1 Entity-Relationship Summary

The schema consists of seven tables: five domain tables (`Recipes`, `RecipeSteps`, `Tags`, `RecipeTags`, `ShareTokens`, `Favorites`), one junction table (`RecipeTags`), and one EF Core system table (`__EFMigrationsHistory`).

### 2.2 Table Definitions

#### Recipes

The central table storing recipe metadata.

```sql
CREATE TABLE "Recipes" (
    "Id"               INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Title"            TEXT    NOT NULL,   -- max 200 characters
    "Description"      TEXT,               -- max 2000 characters
    "PrepTimeMinutes"  INTEGER,
    "CookTimeMinutes"  INTEGER,
    "Servings"         INTEGER,
    "Difficulty"        INTEGER,           -- 0 = Easy, 1 = Medium, 2 = Hard
    "ImageUrl"         TEXT,
    "CreatedAt"        TEXT,               -- ISO 8601 UTC (e.g. "2025-01-15T10:30:00Z")
    "UpdatedAt"        TEXT                -- ISO 8601 UTC
);
```

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | INTEGER | PK, AUTOINCREMENT | Sequential identifier |
| Title | TEXT | NOT NULL, max 200 | Title Case by convention |
| Description | TEXT | max 2000 | Plain text, nullable |
| PrepTimeMinutes | INTEGER | nullable | Minutes of prep time |
| CookTimeMinutes | INTEGER | nullable | Minutes of cook time |
| Servings | INTEGER | nullable | Number of servings |
| Difficulty | INTEGER | nullable | Enum: 0=Easy, 1=Medium, 2=Hard |
| ImageUrl | TEXT | nullable | Relative or absolute URL |
| CreatedAt | TEXT | nullable | ISO 8601 UTC string |
| UpdatedAt | TEXT | nullable | ISO 8601 UTC string |

#### RecipeSteps

Ordered preparation/cooking instructions for each recipe.

```sql
CREATE TABLE "RecipeSteps" (
    "Id"            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "RecipeId"      INTEGER NOT NULL,
    "StepNumber"    INTEGER NOT NULL,
    "Instruction"   TEXT    NOT NULL,      -- max 1000 characters
    "TimerMinutes"  INTEGER,               -- null if no timer needed
    FOREIGN KEY ("RecipeId") REFERENCES "Recipes"("Id") ON DELETE CASCADE,
    UNIQUE ("RecipeId", "StepNumber")
);
```

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | INTEGER | PK, AUTOINCREMENT | Sequential identifier |
| RecipeId | INTEGER | FK to Recipes.Id, NOT NULL | CASCADE delete |
| StepNumber | INTEGER | NOT NULL, UNIQUE with RecipeId | Sequential, starting at 1 |
| Instruction | TEXT | NOT NULL, max 1000 | Single step description |
| TimerMinutes | INTEGER | nullable | Null (not 0) when no timer applies |

#### Tags

Reusable labels for categorizing recipes.

```sql
CREATE TABLE "Tags" (
    "Id"   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT    NOT NULL UNIQUE         -- max 50 characters
);
```

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | INTEGER | PK, AUTOINCREMENT | Sequential identifier |
| Name | TEXT | NOT NULL, UNIQUE, max 50 | Lowercase or Title Case |

#### RecipeTags

Junction table implementing the many-to-many relationship between Recipes and Tags.

```sql
CREATE TABLE "RecipeTags" (
    "RecipeId" INTEGER NOT NULL,
    "TagId"    INTEGER NOT NULL,
    PRIMARY KEY ("RecipeId", "TagId"),
    FOREIGN KEY ("RecipeId") REFERENCES "Recipes"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("TagId")    REFERENCES "Tags"("Id")    ON DELETE CASCADE
);
```

| Column | Type | Constraints | Notes |
|---|---|---|---|
| RecipeId | INTEGER | Composite PK, FK to Recipes.Id | CASCADE delete |
| TagId | INTEGER | Composite PK, FK to Tags.Id | CASCADE delete |

#### ShareTokens

Tokens for sharing individual recipes via URL. This feature is intentionally broken in the starter app - students fix it during the hackathon.

```sql
CREATE TABLE "ShareTokens" (
    "Id"        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "RecipeId"  INTEGER NOT NULL,
    "Token"     TEXT    NOT NULL UNIQUE,
    "CreatedAt" TEXT,                       -- ISO 8601 UTC
    "ExpiresAt" TEXT,                       -- ISO 8601 UTC
    FOREIGN KEY ("RecipeId") REFERENCES "Recipes"("Id") ON DELETE CASCADE
);
```

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | INTEGER | PK, AUTOINCREMENT | Sequential identifier |
| RecipeId | INTEGER | FK to Recipes.Id, NOT NULL | CASCADE delete |
| Token | TEXT | NOT NULL, UNIQUE | GUID or short code |
| CreatedAt | TEXT | nullable | ISO 8601 UTC string |
| ExpiresAt | TEXT | nullable | ISO 8601 UTC string |

#### Favorites

Tracks which recipes a user has favorited. UserId is a simple string - there is no authentication system.

```sql
CREATE TABLE "Favorites" (
    "Id"        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "UserId"    TEXT    NOT NULL,
    "RecipeId"  INTEGER NOT NULL,
    "CreatedAt" TEXT,                       -- ISO 8601 UTC
    FOREIGN KEY ("RecipeId") REFERENCES "Recipes"("Id") ON DELETE CASCADE,
    UNIQUE ("UserId", "RecipeId")
);
```

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | INTEGER | PK, AUTOINCREMENT | Sequential identifier |
| UserId | TEXT | NOT NULL | Arbitrary string, not linked to auth |
| RecipeId | INTEGER | FK to Recipes.Id, NOT NULL | CASCADE delete |
| CreatedAt | TEXT | nullable | ISO 8601 UTC string |

#### __EFMigrationsHistory

System table managed by Entity Framework Core. Tracks which migrations have been applied.

```sql
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId"    TEXT NOT NULL PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);
```

This table is not modified by application code and should not be altered manually.

### 2.3 Relationship Diagram (Text)

```
Recipes (1) ----< (many) RecipeSteps
Recipes (1) ----< (many) RecipeTags >---- (1) Tags
Recipes (1) ----< (many) ShareTokens
Recipes (1) ----< (many) Favorites
```

All foreign keys use `ON DELETE CASCADE`, so deleting a recipe removes its steps, tags, share tokens, and favorites automatically.

---

## 3. Seed Data Specification

### 3.1 Tags (10 total)

The following 10 tags are seeded before any recipe data is inserted:

| Id | Name |
|---|---|
| 1 | Breakfast |
| 2 | Lunch |
| 3 | Dinner |
| 4 | Dessert |
| 5 | Vegetarian |
| 6 | Vegan |
| 7 | Quick |
| 8 | Italian |
| 9 | Asian |
| 10 | Mexican |

### 3.2 Seed Recipes (12 total)

| # | Title | Difficulty | Prep (min) | Cook (min) | Tags | Steps | Timed Steps |
|---|---|---|---|---|---|---|---|
| 1 | Classic Margherita Pizza | Medium | 30 | 15 | Italian, Dinner | 6 | 1 (bake 12m) |
| 2 | Fluffy Pancakes | Easy | 10 | 15 | Breakfast, Quick | 5 | 1 (cook 3m) |
| 3 | Chicken Alfredo Pasta | Medium | 15 | 25 | Italian, Dinner | 7 | 2 (boil 10m, simmer 5m) |
| 4 | Thai Green Curry | Medium | 20 | 20 | Asian, Dinner | 6 | 1 (simmer 15m) |
| 5 | Avocado Toast | Easy | 5 | 3 | Breakfast, Vegetarian, Quick | 4 | 1 (toast 3m) |
| 6 | Beef Tacos | Easy | 15 | 10 | Mexican, Dinner | 5 | 1 (cook 8m) |
| 7 | Chocolate Lava Cake | Hard | 20 | 14 | Dessert | 8 | 1 (bake 14m) |
| 8 | Vegetable Stir-Fry | Easy | 15 | 10 | Asian, Vegetarian, Vegan, Quick | 5 | 1 (stir-fry 5m) |
| 9 | French Onion Soup | Hard | 15 | 60 | Dinner | 7 | 2 (caramelize 45m, broil 3m) |
| 10 | Berry Smoothie Bowl | Easy | 10 | 0 | Breakfast, Vegetarian, Quick | 4 | 0 |
| 11 | Homemade Sushi Rolls | Hard | 45 | 20 | Asian, Dinner | 8 | 1 (cook rice 20m) |
| 12 | Tiramisu | Medium | 30 | 0 | Italian, Dessert | 6 | 0 |

### 3.3 Seed Data Totals

| Entity | Count |
|---|---|
| Recipes | 12 |
| RecipeSteps | ~69 (4-8 per recipe) |
| Tags | 10 |
| RecipeTags | ~26 (varies per recipe) |
| ShareTokens | 0 (intentionally empty) |
| Favorites | 0 (user-generated at runtime) |

### 3.4 Seed Data Rules

- Tags are inserted first via `HasData()` in the EF Core model configuration, ensuring referential integrity.
- ShareTokens is intentionally left empty. The share feature has a bug that students fix during the hackathon.
- Favorites is empty at seed time because favorites are created by user interaction.
- All `CreatedAt` timestamps in seed data use a fixed UTC date (e.g. `2025-01-01T00:00:00Z`) for reproducibility.
- `UpdatedAt` matches `CreatedAt` in seed data.

---

## 4. Data Volume and Performance

### 4.1 Starting Dataset Size

| Metric | Value |
|---|---|
| Total rows across all tables | ~117 |
| Estimated database file size | ~50-80 KB |
| Largest table | RecipeSteps (~69 rows) |

### 4.2 Expected Growth During Hackathon

Students working through Challenge 02 (adding new recipes through the UI or API) may add 5-10 additional recipes. Conservative upper bound for a single hackathon session:

| Metric | Maximum Expected |
|---|---|
| Recipes | ~50 |
| RecipeSteps | ~300 |
| Tags | ~20 |
| RecipeTags | ~100 |
| ShareTokens | ~20 |
| Favorites | ~50 |
| Database file size | ~200 KB |

### 4.3 Performance Characteristics

SQLite is more than adequate for this workload. No performance tuning is needed.

- **Read latency:** Sub-millisecond for all queries at this scale.
- **Write latency:** Sub-millisecond for single inserts. SQLite uses a single-writer lock, which is a non-issue for a single-user hackathon app.
- **Indexing:** Primary keys and unique constraints automatically create indexes. No additional indexes are required for fewer than 50 recipes.
- **Connection pooling:** Not needed. EF Core opens and closes connections per request, which is fine for SQLite in this context.
- **Query complexity:** The most complex query is recipe search with tag filtering, which involves a JOIN across `Recipes`, `RecipeTags`, and `Tags`. At this data volume, a full table scan is faster than index lookups.

---

## 5. Data Quality Prerequisites

These rules must hold true in the seed data and should be validated if students modify the seeding logic.

### 5.1 Title Formatting

All recipe titles use Title Case (e.g. "Chicken Stir Fry", not "chicken stir fry" or "CHICKEN STIR FRY"). This is a convention, not enforced by a database constraint.

### 5.2 Step Number Sequencing

Step numbers for each recipe must be sequential integers starting at 1 with no gaps:

```
Recipe 1: StepNumber 1, 2, 3, 4, 5, 6       -- correct
Recipe 1: StepNumber 1, 2, 4, 5              -- incorrect (gap at 3)
Recipe 1: StepNumber 0, 1, 2, 3              -- incorrect (starts at 0)
```

The `UNIQUE ("RecipeId", "StepNumber")` constraint prevents duplicate step numbers within a recipe but does not enforce sequential ordering.

### 5.3 Timer Values

- Steps that include a timer must have `TimerMinutes` set to a positive integer.
- Steps without a timer must have `TimerMinutes` set to `NULL`, not `0`.
- A value of `0` would be misleading in the UI (displaying a 0-minute timer).

### 5.4 Tag Seeding Order

Tags must be seeded before `RecipeTags` rows are inserted. EF Core `HasData()` handles this correctly when tags and recipe-tag associations are configured in the model builder. If students switch to manual SQL seeding, they must maintain this insertion order:

1. `Tags`
2. `Recipes`
3. `RecipeSteps`
4. `RecipeTags`

### 5.5 ShareTokens - Intentionally Empty

The `ShareTokens` table must contain zero rows in the seed data. The share-by-link feature is broken by design in the starter app (this is one of the bugs students diagnose and fix). Pre-seeding tokens would mask the bug.

### 5.6 String Length Enforcement

SQLite does not enforce `VARCHAR(n)` length limits at the database level - all `TEXT` columns accept strings of any length. Length validation is handled by EF Core model configuration and data annotations in the .NET layer:

| Column | Max Length | Enforced By |
|---|---|---|
| Recipes.Title | 200 | `[MaxLength(200)]` / `HasMaxLength(200)` |
| Recipes.Description | 2000 | `[MaxLength(2000)]` / `HasMaxLength(2000)` |
| RecipeSteps.Instruction | 1000 | `[MaxLength(1000)]` / `HasMaxLength(1000)` |
| Tags.Name | 50 | `[MaxLength(50)]` / `HasMaxLength(50)` |

---

## 6. SQLite Limitations and Workarounds

### 6.1 Case Sensitivity in Queries

SQLite string comparison with `=` is case-sensitive by default:

```sql
-- Returns NO results if the stored value is "Breakfast"
SELECT * FROM Tags WHERE Name = 'breakfast';

-- LIKE is case-insensitive for ASCII characters
SELECT * FROM Tags WHERE Name LIKE 'breakfast';  -- matches "Breakfast"
```

This is directly relevant to **Bug 2** in the hackathon, where a case-sensitive comparison in tag filtering causes search results to be missed. Students must use `LIKE`, `COLLATE NOCASE`, or normalize casing in the application layer.

**EF Core workaround:** Use `EF.Functions.Like()` or `.ToLower()` in LINQ queries:

```csharp
// Case-insensitive tag lookup
.Where(t => EF.Functions.Like(t.Name, tagName))

// Alternative: normalize both sides
.Where(t => t.Name.ToLower() == tagName.ToLower())
```

### 6.2 No Native DateTimeOffset Support

SQLite has no built-in date/time column types. EF Core stores `DateTime` values as ISO 8601 text strings (e.g. `"2025-01-15T10:30:00Z"`). This means:

- Date comparisons use string sorting, which works correctly for ISO 8601 format.
- `DateTimeOffset` is not natively supported. Use `DateTime` in UTC instead.
- The `datetime()` SQLite function can parse these strings if raw SQL is needed.

### 6.3 No Concurrent Write Access

SQLite uses a file-level lock for writes. Only one connection can write at a time. This is a non-issue for a single-user hackathon demo. If two browser tabs submit recipes simultaneously, one write will block briefly until the other completes (milliseconds at this scale).

### 6.4 No Stored Procedures or Functions

SQLite has no support for stored procedures, triggers (though supported, not used here), or user-defined functions at the SQL level. All business logic (validation, computed values, token generation) lives in the .NET application layer.

### 6.5 Migration Limitations

SQLite has limited `ALTER TABLE` support:

- Adding a column: supported.
- Renaming a column: supported (SQLite 3.25+).
- Dropping a column: supported (SQLite 3.35+).
- Changing a column type or constraint: not supported. Requires creating a new table, copying data, dropping the old table, and renaming.

EF Core handles table rebuilds automatically in migrations, but complex schema changes may generate verbose migration code. For a hackathon, the simplest recovery path is deleting `api/recipes.db` and re-running the app to recreate it from scratch.

### 6.6 No Enum Type

The `Difficulty` column stores integer values (0, 1, 2) rather than enum names. The mapping is:

| Value | Label |
|---|---|
| 0 | Easy |
| 1 | Medium |
| 2 | Hard |

EF Core handles the C# enum-to-integer conversion automatically. The API returns the string label; the database stores the integer.

---

## 7. Data Privacy and Compliance

### 7.1 No Personal Data

RecipeHub does not collect, store, or process any personally identifiable information (PII):

| Data Element | PII Classification | Notes |
|---|---|---|
| UserId (Favorites table) | Not PII | Arbitrary string identifier, not linked to any authentication system or real identity |
| Recipe content | Not PII | Food recipes authored by users |
| ShareTokens | Not PII | Random GUIDs with no user association |
| Timestamps | Not PII | System-generated UTC timestamps |

### 7.2 No Authentication

There is no user registration, login, password storage, or session management. The `UserId` field in the `Favorites` table is a client-provided string (e.g. `"user1"`) with no verification. This is acceptable for a local hackathon demo.

### 7.3 Data Retention

There are no data retention requirements. The SQLite database file is disposable:

- Delete `api/recipes.db` to reset all data.
- Restart the application to recreate the database with seed data.
- No backup or archival process is needed.

### 7.4 Regulatory Compliance

| Regulation | Applicability |
|---|---|
| GDPR | Not applicable - no PII, no real users, no data processing agreements needed |
| CCPA | Not applicable - no consumer data |
| HIPAA | Not applicable - no health data |
| SOC 2 | Not applicable - no production deployment |
| Data residency | Not applicable - runs locally on the student's machine |

### 7.5 Data Protection Impact Assessment (DPIA)

A DPIA is not required. The application processes no personal data, has no external network communication, and runs entirely on localhost.

---

## 8. Integration Points

### 8.1 Starter Application (Challenges 01-05)

The starter app has zero external integrations. All data operations are local:

```
Browser (React 19)  <-->  .NET 10 Web API  <-->  SQLite (recipes.db)
```

- No external APIs are called.
- No webhooks are sent or received.
- No event streams, message queues, or pub/sub patterns.
- No external databases or caches.

### 8.2 Potential Integration in Challenge 06 (Ralph)

Challenge 06 may introduce a GitHub API integration where students use GitHub Copilot to build a feature that exports recipe data or interacts with an external service. If implemented:

| Aspect | Detail |
|---|---|
| External service | GitHub REST API (`api.github.com`) |
| Authentication | Personal access token (PAT), provided by the student |
| Data flow | Outbound only (app sends data to GitHub) |
| Data sensitivity | Recipe content only - no PII |
| Failure mode | External API unavailability should not break core app functionality |

This integration is optional and student-implemented. The starter codebase contains no GitHub API client code.

### 8.3 No Background Services

The application has no background jobs, scheduled tasks, or worker processes. All operations are synchronous request-response cycles through the Web API.

---

## Appendix: Quick Reference

### Database Reset Procedure

```bash
# Stop the application, then:
rm api/recipes.db
dotnet run
# EF Core recreates the database and applies seed data on startup
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=recipes.db"
  }
}
```

### EF Core Commands (for reference)

```bash
# Apply migrations (usually automatic on startup)
dotnet ef database update

# Add a new migration after model changes
dotnet ef migrations add <MigrationName>

# Generate SQL script for review
dotnet ef migrations script
```
