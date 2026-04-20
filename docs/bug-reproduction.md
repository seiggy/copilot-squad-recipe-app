# Bug Reproduction Checklist

> ⚠️ **SPOILERS — Coach Reference Only**
>
> This document contains bug locations, root causes, and fix hints for Challenge 05.
> Do NOT share with participants until after the hackathon.

---

## Challenge 05 Bugs

### BUG-001: Cook Mode Off-By-One

**ID:** BUG-001  
**Severity:** Medium  
**Feature:** Cook Mode step-by-step navigation

**Files & Line Numbers:**
- Backend: `src/RecipeHub.Api/Endpoints/CookModeEndpoints.cs` line 37
- Frontend: `src/RecipeHub.Web/src/hooks/useCookMode.ts` line 19

**Root Cause:**
- Backend: `s.StepNumber == stepNumber - 1` — applies an off-by-one adjustment to 1-indexed step data
- Frontend: `useState(1)` initializes at step 1, but the backend query returns step 0 data

**User-Observable Behavior:**
1. Open any recipe with multiple steps
2. Click "Cook Mode" or navigate to `/recipes/:id/cook`
3. Step 1 displays the content of step 2
4. Cannot reach the last step — clicking Next on step N-1 shows "not found"

**Failing Test (un-skip to verify):**
- `tests/RecipeHub.Api.Tests/Bugs/BugCookModeOffByOneTests.cs`
  - `GetStep1_ShouldReturnFirstStep_NotSecond`
  - `GetStep2_ShouldReturnSecondStep`
  - `GetStepBeyondTotal_ShouldReturn404`

**Fix Hint:**
Remove the `- 1` in `CookModeEndpoints.cs` line 37. The step numbers in the database are already 1-indexed; no adjustment needed.

---

### BUG-002: Search Case Sensitivity

**ID:** BUG-002  
**Severity:** Medium  
**Feature:** Recipe search

**Files & Line Numbers:**
- Backend: `src/RecipeHub.Api/Endpoints/SearchEndpoints.cs` line ~18-22

**Root Cause:**
- `string.Contains()` in LINQ to SQLite is case-sensitive by default
- Queries like "chicken" don't match "Chicken Alfredo"

**User-Observable Behavior:**
1. Navigate to `/recipes` or use the search bar
2. Search for "chicken" (lowercase)
3. Returns 0 results even though "Chicken Alfredo" exists
4. Search for "Chicken" (exact case) works correctly

**Failing Test (un-skip to verify):**
- `tests/RecipeHub.Api.Tests/Bugs/BugSearchCaseSensitivityTests.cs`
  - `Search_WithLowercaseQuery_MatchesMixedCaseTitle`
  - `Search_WithUppercaseQuery_MatchesLowercaseTitle`
  - `Search_IsCaseInsensitiveAcrossTitleAndDescription`

**Fix Hint:**
Use `EF.Functions.Like()` with `%` wildcards, or call `.ToLower()` on both the query and the field. Example: `.Where(r => r.Title.ToLower().Contains(query.ToLower()))`

---

### BUG-003: Share Token Persistence

**ID:** BUG-003  
**Severity:** High  
**Feature:** Recipe sharing via token URL

**Files & Line Numbers:**
- Backend: `src/RecipeHub.Api/Endpoints/ShareEndpoints.cs` line ~12-20

**Root Cause:**
- `SaveChangesAsync()` is called BEFORE the `Token` property is assigned
- The token is generated, but the entity is saved with `Token = null`
- All subsequent GET `/api/shared/{token}` requests return 404

**User-Observable Behavior:**
1. Open any recipe detail page
2. Click "Share" button
3. A share URL is displayed (e.g., `/shared/abc123`)
4. Copy the URL and open it in a new tab
5. Page shows 404 — the token was never persisted

**Failing Test (un-skip to verify):**
- `tests/RecipeHub.Api.Tests/Bugs/BugSharePersistenceTests.cs`
  - `PostShare_ShouldPersistTokenToDatabase`
  - `GetSharedToken_ShouldReturnRecipe`

**Fix Hint:**
Move the `Token = Guid.NewGuid().ToString("N")` assignment BEFORE `SaveChangesAsync()`. The token must be set before the entity is persisted.

---

## Challenge 03 Formatting Inconsistencies

These are intentional stylistic issues for participants to find using linters/formatters.

### FMT-001: Trailing Whitespace in README

**File:** `README.md`  
**Lines:** 11, 40  
**Type:** Trailing whitespace after heading text  
**Detection:** `markdownlint` rule MD009, or `git diff --check`

### FMT-002: Inconsistent Brace Style in TestBase.cs

**File:** `tests/RecipeHub.Api.Tests/TestBase.cs`  
**Lines:** 52-60 (`TryDeleteDb` method)  
**Type:** K&R brace style (opening `{` on same line) when rest of codebase uses Allman style (opening `{` on new line)  
**Detection:** `dotnet format --verify-no-changes` or `.editorconfig` csharp_new_line_before_open_brace setting

---

## Verification Commands

```bash
# Run all backend tests (9 skipped = 3 bugs × 2-3 tests each + 1 placeholder)
dotnet test

# Run frontend tests (3 skipped = 2 bug tests + 1 placeholder)
cd src/RecipeHub.Web && npm run test

# Check for formatting issues
dotnet format --verify-no-changes
cd src/RecipeHub.Web && npm run lint
```

---

*Last updated: 2026-04-20*
