---
name: react-writing-plans
description: Use when you have a spec, PRD, or requirements for a React frontend feature requiring multiple components, stores, or API integrations, before writing any code. Creates concise implementation plans that reference architecture contracts.
---

# React Writing Plans

## Overview

Plans are **architectural blueprints**, not copy-paste code. Each task references contracts defined in the architecture doc. Implementers use TDD skills to fill in the actual code.

**Announce at start:** "I'm using the react-writing-plans skill to create the implementation plan."

**Save plans to:** `_docs/plans/YYYY-MM-DD-<feature-name>.md`

## When to Use
- Multi-step React feature requiring new components, stores, or API integration
- PRD or spec exists but no implementation plan
- Feature touches 3+ files or requires coordinated changes
- Need to hand off implementation to another engineer or subagent

## When NOT to Use
- Single-file bug fix with obvious cause and fix
- Config/environment changes (vite.config, tsconfig, tailwind.config)
- Renaming or moving files without logic changes
- Adding an npm package with no code changes beyond the import

## Plan Document Header

**Every plan MUST start with this header:**

```markdown
# [Feature Name] Implementation Plan

> **For Claude:** **REQUIRED SUB-SKILL:** Use `saurun:react-tdd` to implement this plan task-by-task with TDD.

**Goal:** [One sentence describing what this builds]

**Architecture:** `_docs/specs/{DATE}-{feature}-architecture.md`

**Tech Stack:** React 19, Vite, TypeScript, Tailwind CSS v4, Zustand, Vitest, React Testing Library, MSW

---
```

## Task Structure

```markdown
### Task N: [Name]

**Implements:** [Contract reference from Architecture doc, e.g., "AddItemForm (Architecture §Component Tree)"]

**Files:**
- Create: `src/components/exact/path/Component.tsx`
- Test: `src/components/exact/path/__tests__/Component.test.tsx`

**Behaviors:**
- [Happy path behavior]
- [Error case 1]
- [Error case 2]

**Dependencies:** Task X (if applicable)
```

## Frontend Task Examples

### Component Task

```markdown
### Task 3: AddItemForm component

**Implements:** AddItemForm (Architecture §Component Tree)

**Files:**
- Create: `src/components/AddItemForm.tsx`
- Test: `src/components/__tests__/AddItemForm.test.tsx`

**Behaviors:**
- Submitting valid input calls onItemAdded with new item
- Empty name shows validation error
- Displays loading state while submitting
- Disables submit button when form invalid
```

### Store Task

```markdown
### Task 2: useListStore Zustand store

**Implements:** useListStore (Architecture §Stores)

**Files:**
- Create: `src/stores/listStore.ts`
- Test: `src/stores/__tests__/listStore.test.ts`

**Behaviors:**
- fetchLists populates lists array from API
- createList adds new list and returns it
- deleteList removes list from array
- Handles API errors by setting error state
```

### Page Task

```markdown
### Task 5: ListDetailPage

**Implements:** /lists/{id} → ListDetailPage (Architecture §Pages)

**Files:**
- Create: `src/pages/ListDetailPage.tsx`
- Test: `src/pages/__tests__/ListDetailPage.test.tsx`

**Behaviors:**
- Shows loading spinner while fetching
- Displays list name and items when loaded
- Shows error message when list not found
- AddItemForm adds items to the list

**Dependencies:** Task 2 (useListStore), Task 3 (AddItemForm)
```

### API Hook Task

```markdown
### Task 4: useListQuery hook

**Implements:** useListQuery (Architecture §API Hooks)

**Files:**
- Create: `src/hooks/useListQuery.ts`
- Test: `src/hooks/__tests__/useListQuery.test.ts`

**Behaviors:**
- Returns loading state initially
- Returns list data when API succeeds
- Returns error when API fails
- Refetches on listId change
```

## What Plans Include

| Element | Required |
|---------|----------|
| Exact file paths (Create/Modify/Test) | ✓ |
| Contract reference (`Implements:`) | ✓ |
| Behaviors (one line each) | ✓ |
| Task dependencies | When applicable |

## What Plans Do NOT Include

| Element | Reason |
|---------|--------|
| Full test code | TDD skill generates tests from behaviors |
| Full implementation code | Implementer writes from contract + behaviors |
| Step-by-step TDD instructions | TDD skill handles workflow |
| Expected failure messages | TDD skill handles verification |
| "What bugs does this catch?" table | Behaviors implicitly define bug coverage |

## Test Infrastructure Task

**Task 1 of every plan MUST set up test infrastructure:**

```markdown
### Task 1: Test infrastructure setup

**Implements:** Shared test helpers (N/A - infrastructure)

**Files:**
- Create: `src/test-utils.tsx`
- Create: `src/mocks/server.ts`
- Create: `src/mocks/handlers.ts`
- Modify: `vitest.setup.ts`

**Behaviors:**
- renderWithProviders wraps components with necessary providers
- MSW server configured with onUnhandledRequest: 'error'
- Zustand store reset in beforeEach
```

## Tailwind v4 Compliance

**Implementers MUST follow these rules (remind in plan if UI-heavy):**

- CSS variables use **parentheses**: `bg-(--brand-color)` NOT `bg-[--brand-color]`
- Use renamed utilities: `shadow-xs`, `rounded-xs` (not `shadow-sm`, `rounded-sm` for smallest)
- Class merging: always `cn()`, never template literals
- No `theme()` in arbitrary values — use `var(--color-*)` instead

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Writing full test/implementation code | Just list behaviors — TDD skill writes code |
| Forgetting `Implements:` reference | Every task MUST reference architecture contract |
| Vague behaviors like "handles errors" | Be specific: "Shows error message when API returns 500" |
| Missing file paths | Every task MUST list exact Create/Modify/Test paths |
| Tasks too large (>3 files) | Split into smaller tasks |
| No test infrastructure in Task 1 | `renderWithProviders` + MSW setup MUST be Task 1 |
| Square bracket CSS vars | Use `bg-(--var)` NOT `bg-[--var]` |

## Completion

After saving the plan, report:

**"Plan saved to `_docs/plans/<filename>.md`. Ready for execution."**
