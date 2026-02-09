

Here is the dispatch prompt for the code-reviewer subagent with the React test quality criteria applied:

---

**Dispatch via Task tool targeting `superpowers:code-reviewer`:**

```
Use template at requesting-code-review/code-reviewer.md

WHAT_WAS_IMPLEMENTED: Added ProductFilter component with category dropdown, price range slider, availability checkbox. Connected to Zustand productStore for filter state. Added ProductFilter.test.tsx with 15 tests covering filter application, reset, mobile responsive, and accessibility.

PLAN_OR_REQUIREMENTS: Task 3 from _docs/plans/products-feature.md

BASE_SHA: aaa111bbb

HEAD_SHA: ccc222ddd

DESCRIPTION: ProductFilter component with category/price/availability filtering, Zustand integration, and test suite

ADDITIONAL_REVIEW_CRITERIA: |
  ## React Test Quality Review Checklist

  Review ALL test files in the diff. For each test, apply these checks:

  ### Behavioral Testing
  - [ ] No className/style tests (flag: test asserts on `toHaveClass`, `toHaveStyle`, CSS variable values)
  - [ ] No getter/setter tests (flag: `can set`, `can get`, `has default`, `is nullable`)
  - [ ] Every test catches a real bug — ask "what bug does this prevent?" If answer is "none", flag it
  - [ ] Tests verify user-visible outcomes (text, roles, store state), not mock interactions (`toHaveBeenCalled`, `vi.fn()`)
  - [ ] No assertion-less tests (every test has at least one `expect`)
  - [ ] No "renders without crashing" tests — zero-assertion smoke tests prove nothing

  ### Test Structure
  - [ ] Max 3 assertions per test (single logical assertion OK even if multiple expect calls)
  - [ ] Naming follows `it('should [expected behavior] when [condition]')`
  - [ ] No "and" in test names — split into separate tests
  - [ ] `it.each`/`describe.each` used when testing same behavior with multiple inputs
  - [ ] One behavior per test — a test named "removes and clears" is two tests

  ### Test Infrastructure
  - [ ] Shared custom render wrapper used (not inline provider wrapping per test)
  - [ ] Zustand stores reset in `beforeEach` using the store's designated reset action or `useStore.setState(initialState)` — do NOT use `setState` with test values then assert those same values
  - [ ] No test-only props or exports in production components (no `data-testid` when accessible queries work)
  - [ ] MSW `setupServer` in shared setup file, not duplicated per test
  - [ ] `vitest.setup.ts` configures MSW lifecycle (`beforeAll/afterEach/afterAll`)
  - [ ] `onUnhandledRequest: 'error'` set in MSW server.listen()

  ### Mock Boundaries
  - [ ] API boundaries mocked via MSW (`http.get`, `http.post`, etc.)
  - [ ] Zustand stores NOT mocked — use real stores with controlled state
  - [ ] React components NOT mocked — render real component tree
  - [ ] Custom hooks NOT mocked — test through component rendering
  - [ ] `vi.mock` used ONLY for browser APIs (`window.location`, `IntersectionObserver`, timers)
  - [ ] Mock setup is <50% of test code

  ### Coverage
  - [ ] Edge cases tested (null, undefined, empty arrays, boundary values)
  - [ ] Error paths tested (API failures, invalid input, not found)
  - [ ] All new components/hooks have at least one test
  - [ ] Happy path + at least one failure path per user flow

  ### React 19 Specifics
  - [ ] `useFormStatus` used correctly (only in child of `<form action>`)
  - [ ] `use()` for promises/context used in render (not in callbacks/effects)
  - [ ] Server Actions properly handled (if applicable)
  - [ ] `useOptimistic` used for optimistic UI updates (if applicable)

  ### Tailwind v4 Compliance
  - [ ] No v3 CSS variable syntax: `[--var]` → must be `(--var)`
  - [ ] Renamed utilities correct: `shadow`→`shadow-sm`, `rounded`→`rounded-sm`, `blur`→`blur-sm`
  - [ ] No `ring-offset-*` (deprecated) → use `outline-*` + `outline-offset-*`
  - [ ] No `theme()` in arbitrary values → use `var(--color-*)` directly
  - [ ] `cn()` used for class merging (not template literals)
  - [ ] No `!prefix` important modifier → use `suffix!`

  ### Zustand Store Review
  - [ ] Selectors used to prevent unnecessary re-renders: `useStore((s) => s.field)` not `useStore()`
  - [ ] No derived state stored (compute in selector or component)
  - [ ] Actions don't read stale state (use `get()` inside `set()` callback)

  ### Accessibility (WCAG 2.1 AA)
  - [ ] Interactive elements have accessible names (aria-label or visible text)
  - [ ] Focus states visible: `focus-visible:outline-2 focus-visible:outline-offset-2`
  - [ ] Color contrast meets 4.5:1 for text
  - [ ] Form fields have associated labels
  - [ ] Dynamic content changes announced (aria-live, aria-busy)
  - [ ] Keyboard navigation works (Tab order, Enter/Space activation)

  ### Anti-Patterns to Flag
  Flag these by name in review:
  - **CanSetProperties**: Test that only verifies state assignment or prop values
  - **ClassNameTest**: Test that asserts on `toHaveClass` or CSS classes
  - **RendersSmokeTest**: Test with zero assertions ("renders without crashing")
  - **MockVerifyOnly**: Test where only assertion is `toHaveBeenCalled` or `vi.fn()` verification
  - **AssertionOverload**: Test with >3 unrelated assertions (Important); >5 unrelated is Critical
  - **DualBehavior**: Test name contains "and" — testing two things (e.g., "removes and clears")
  - **StoreMock**: `vi.mock` on Zustand store, component, or custom hook
  - **Assertionless**: Test with zero `expect()` calls — passes silently, catches nothing
  - **DirectSetState**: Test that uses `useStore.setState()` to SET values then asserts those same values
  - **ProviderDuplicate**: Provider wrapping copy-pasted across test files instead of shared render
  - **V3Syntax**: Tailwind v3 syntax in v4 project (brackets for CSS vars, old utility names)

  ### Severity Guide
  - **Critical**: className/getter tests, store/component mocking, no tests for new components, >5 unrelated assertions, assertion-less tests, v3 CSS variable syntax `[--var]`
  - **Important**: missing it.each, >3 unrelated assertions, duplicate provider setup, mock-verify-only, test naming violations, "and" in test names, missing accessibility labels
  - **Minor**: naming convention inconsistency, missing edge case test, magic numbers in test data
```

---

**What this does:**

- Targets the `superpowers:code-reviewer` agent, which loads the base `requesting-code-review/code-reviewer.md` template
- The base template handles git diff inspection between `aaa111bbb..ccc222ddd`, standard code quality checks, and structured output format
- `ADDITIONAL_REVIEW_CRITERIA` injects the full React test quality checklist on top of the base review
- The reviewer will return: Strengths, Issues (Critical/Important/Minor with named anti-patterns), Recommendations, and Assessment with merge verdict

**Files the reviewer will inspect:**
- `ProductFilter.tsx` — component implementation
- `ProductFilter.test.tsx` — 15 tests (primary focus of the quality checklist)
- `productStore.ts` — Zustand store changes

**Key things the reviewer will catch specific to this implementation:**
- Whether the 15 tests are behavioral (user-visible outcomes) vs structural (className/getter)
- Whether the Zustand productStore is tested via real store usage or mocked out
- Whether the price range slider and category dropdown tests verify user interaction outcomes
- Tailwind v4 compliance in the component markup
- Zustand selector usage to prevent re-renders
- Accessibility of the dropdown, slider, and checkbox elements