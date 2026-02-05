---
name: react-writing-plans
description: Use when you have a spec, PRD, or requirements for a React frontend feature requiring multiple components, stores, or API integrations, before writing any code.
---

# React Writing Plans

## Overview

Every implementation plan is a self-contained instruction set: an engineer with zero codebase context should be able to execute it task-by-task using only the plan and TDD.

Write bite-sized tasks with exact file paths, complete code, and explicit test specs. DRY. YAGNI. TDD. Frequent commits. Assume the implementer is skilled but knows nothing about our toolset, problem domain, or good test design — be explicit about what to test and what NOT to test.

**Announce at start:** "I'm using the react-writing-plans skill to create the implementation plan."

**REQUIRED:** Run in a dedicated worktree created by `superpowers:brainstorming`.

**Save plans to:** `docs/plans/YYYY-MM-DD-<feature-name>.md`

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

## Quick Reference

| Element | Requirement |
|---------|-------------|
| **Plan location** | `docs/plans/YYYY-MM-DD-<feature-name>.md` |
| **Task 1** | Test infrastructure (renderWithProviders, MSW, Zustand reset) |
| **Task granularity** | One behavior, max 2 new files |
| **Each task** | 5 steps: write test → run (fail) → implement → run (pass) → commit |
| **File paths** | Exact paths with `Create:` / `Modify:` / `Test:` |
| **Test naming** | `it('should [behavior] when [condition]')` |
| **Max assertions** | 3 per test, use `it.each` for multiple inputs |
| **Mock boundary** | MSW for APIs, `vi.mock` for browser APIs only |
| **Never mock** | Zustand stores, React components, custom hooks |
| **Tailwind v4** | Parentheses for CSS vars: `bg-(--var)` not `bg-[--var]` |

## Test Quality Rules for Plan Writers

**These rules apply to EVERY test you spec in a plan. Violations mean the plan is broken.**

### NEVER spec these tests:
- "Renders without crashing" smoke tests
- Tests that assert className or style presence
- Tests that verify a prop is passed through
- Tests that check default state values from a store (`hasDefaultItems`, `initialCountIsZero`)
- Tests that mock Zustand stores, React components, or custom hooks
- Assertion-less tests (render + no expect)

### Every test MUST:
- **Verify behavior that could have a bug.** Ask: "What bug does this catch?" If the answer is "none" — delete the test from the plan.
- **Have max 3 assertions.** More than 3? Split into separate tests.
- **Use `it.each` for multiple inputs** of the same behavior (e.g., empty string, null, various invalid inputs).
- **Follow naming: `it('should [behavior] when [condition]')`** — not `test1`, `it works`, `renders component`.
- **No "and" in test names.** If you write "and", split into two tests.

### Phase 1 of every plan MUST include:
- Shared test infrastructure: custom `renderWithProviders` wrapper (`test-utils.tsx`)
- MSW setup: `setupServer` in shared setup file, `vitest.setup.ts` with MSW lifecycle
- `onUnhandledRequest: 'error'` in MSW config (catch missing handlers)
- Zustand store reset pattern in `beforeEach`

### Mock Boundary Rule in plans:
- Spec mocking for: API endpoints via MSW (`http.get`, `http.post`, etc.), browser APIs (`window.location`, `navigator.*`, timers, `IntersectionObserver`)
- NEVER spec mocking for: Zustand stores (use real stores with controlled state), React components, custom hooks, utility functions, domain logic
- Mock setup must be <50% of test code — if mocking dominates, the test is wrong

### "What Bug Does This Catch?" Table

Every plan MUST include a table mapping tests to real bugs prevented:

```markdown
| Test | Bug It Catches |
|------|---------------|
| `should show error when API returns 500` | User sees blank screen on server error |
| `should disable submit when form invalid` | User submits incomplete data |
```

## Bite-Sized Task Granularity

**Each step is one action:**
- "Write the failing test" — step
- "Run it to make sure it fails" — step
- "Implement the minimal code to make the test pass" — step
- "Run the tests and make sure they pass" — step
- "Commit" — step

## Plan Document Header

**Every plan MUST start with this header:**

```markdown
# [Feature Name] Implementation Plan

> **For Claude:** **REQUIRED SUB-SKILL:** Use `saurun:react-tdd` to implement this plan task-by-task with TDD.

**Goal:** [One sentence describing what this builds]

**Architecture:** [2-3 sentences about approach]

**Tech Stack:** React 19, Vite, TypeScript, Tailwind CSS v4, Zustand, Vitest, React Testing Library, MSW

---
```

## Task Structure

```markdown
### Task N: [Component Name]

**Files:**
- Create: `src/components/exact/path/Component.tsx`
- Create: `src/stores/exactStore.ts`
- Test: `src/components/exact/path/__tests__/Component.test.tsx`

**Step 1: Write the failing test**

```tsx
import { renderWithProviders, screen } from '@/test-utils'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { server } from '@/mocks/server'
import { AddItemForm } from '../AddItemForm'

it('should add item to list when form submitted with valid input', async () => {
  const user = userEvent.setup()
  server.use(
    http.post('/api/items', () => HttpResponse.json({ id: '1', name: 'Milk' }))
  )

  renderWithProviders(<AddItemForm listId="list-1" />)

  await user.type(screen.getByRole('textbox', { name: /item name/i }), 'Milk')
  await user.click(screen.getByRole('button', { name: /add/i }))

  expect(await screen.findByText('Milk')).toBeInTheDocument()
})
```

**Step 2: Run test to verify it fails**

Run: `npx vitest run --reporter=verbose src/components/exact/path/__tests__/Component.test.tsx`
Expected: FAIL with "Unable to find element" or "module not found"

**Step 3: Write minimal implementation**

```tsx
export function AddItemForm({ listId }: { listId: string }) {
  const [name, setName] = useState('')
  const addItem = useItemStore((s) => s.addItem)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    await addItem(listId, name)
    setName('')
  }

  return (
    <form onSubmit={handleSubmit}>
      <label>
        Item name
        <input value={name} onChange={(e) => setName(e.target.value)} />
      </label>
      <button type="submit">Add</button>
    </form>
  )
}
```

**Step 4: Run test to verify it passes**

Run: `npx vitest run --reporter=verbose src/components/exact/path/__tests__/Component.test.tsx`
Expected: PASS

**Step 5: Commit**

```bash
git add src/components/exact/path/ src/stores/
git commit -m "feat: add item form with API integration"
```
```

## Tailwind v4 Compliance in Plans

**Every plan that includes UI code MUST follow these rules:**

- CSS variables use **parentheses**: `bg-(--brand-color)` NOT `bg-[--brand-color]`
- Use renamed utilities: `shadow-xs` (not `shadow-sm` for smallest), `rounded-xs` (not `rounded-sm` for smallest)
- Focus rings: `focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--ring-color)`
- Class merging: always `cn()`, never template literals
- Variants: `cva` with `VariantProps` typing
- No `theme()` in arbitrary values — use `var(--color-*)` instead

## Integration Test Pattern

Integration tests render full component trees with real Zustand stores and MSW handlers. Arrange via `userEvent` interactions and MSW response setup. Assert user-visible outcomes (text, roles, navigation). Same naming: `it('should [behavior] when [condition]')`.

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Tasks too large (multiple components + tests in one task) | One behavior per task. If task has >2 new files, split it. |
| Missing file paths | Every task MUST list exact `Create:` / `Modify:` / `Test:` paths. |
| Vague steps like "add validation" | Write the actual code in the plan. No placeholders. |
| Missing expected failure messages | Step 2 must say exactly what the test runner prints on failure. |
| Speccing "renders without crashing" tests | Ask "what bug does this catch?" If "none" — delete. |
| No test infrastructure in Phase 1 | `renderWithProviders` + MSW setup + Zustand reset MUST be Task 1. |
| Mocking Zustand stores | Use real stores. Control state via store actions or `setState`. |
| Square bracket CSS vars | Use `bg-(--var)` NOT `bg-[--var]`. Brackets silently fail in v4. |
| Mocking React components or hooks | Test real components. Mock only API (MSW) and browser APIs. |

## Execution Handoff

After saving the plan, offer execution choice:

**"Plan complete and saved to `docs/plans/<filename>.md`. Two execution options:**

**1. Subagent-Driven (this session)** — I dispatch fresh subagent per task using `saurun:react-implementer-prompt`, review between tasks with `saurun:react-code-quality-reviewer-prompt`, fast iteration

**2. Parallel Session (separate)** — Open new session in worktree, batch execution with checkpoints

**Which approach?"**

**If Subagent-Driven chosen:**
- **REQUIRED:** Use `saurun:react-implementer-prompt` for implementer dispatch
- **REQUIRED:** Use `saurun:react-code-quality-reviewer-prompt` for code review
- Stay in this session, fresh subagent per task + code review

**If Parallel Session chosen:**
- Guide them to open new session in worktree
- **REQUIRED:** New session uses `superpowers:executing-plans`
