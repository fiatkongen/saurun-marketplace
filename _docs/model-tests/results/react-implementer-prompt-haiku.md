Here's the dispatch prompt for your React implementer subagent:

```
Task tool (saurun:frontend-implementer):
  description: "Implement Task 3: Add Product Filter Component"
  prompt: |
    You are implementing Task 3: Add Product Filter Component

    ## Task Description

    Create a ProductFilter component in components/products/ with the following features:
    - Filter by category (dropdown), price range (dual slider), and availability (checkbox)
    - Use Zustand productStore for filter state
    - Apply filters on client side (filter products array)
    - Reset button clears all filters
    - Mobile responsive (stack vertically on sm)

    ## Context

    This task builds on Task 1 (ProductList component), which is already complete. The ProductFilter component will control which products are displayed in the list.

    The component should:
    - Read filter state from the productStore (category, priceRange, availability)
    - Dispatch filter updates to the store when user interacts with controls
    - Be placed alongside ProductList in the product page layout
    - Handle the dual-slider UX for price range selection
    - Integrate seamlessly with the existing product display

    Store shape assumption: productStore has filter state (category, priceRange: {min, max}, availability) and an action to update filters.

    ## Before You Begin

    If you have questions about:
    - The productStore shape or existing filter actions
    - How the filter state should integrate with ProductList
    - The price range slider library or implementation approach
    - Mobile breakpoint definitions or expected layout
    - Anything unclear in the task description

    **Ask them now.** Raise any concerns before starting work.

    ## Your Job

    Once you're clear on requirements:
    1. Implement exactly what the task specifies
    2. **REQUIRED SUB-SKILL:** Follow saurun:react-tdd strictly. No exceptions.
    3. **REQUIRED SUB-SKILL:** Follow saurun:react-tailwind-v4-components for all styling.
    4. Verify implementation works: `npx vitest run`
    5. Commit after each green test cycle
    6. Self-review (see below)
    7. Report back

    Work from: /Users/dev/shopping-app/frontend

    **Stack:** React 19 + Vite + TypeScript + Tailwind v4 + Zustand + Vitest + RTL + MSW

    **While you work:** If you encounter something unexpected or unclear, **ask questions**.
    It's always OK to pause and clarify. Don't guess or make assumptions.

    ## TDD Workflow

    **REQUIRED SUB-SKILL:** Follow saurun:react-tdd for the full Red-Green-Refactor cycle. No shortcuts, no skipping steps.

    ## Before Reporting Back: Self-Review

    Review your work with fresh eyes. Ask yourself:

    **Completeness:**
    - Did I fully implement all filter types (category, price range, availability)?
    - Does the reset button clear all filters?
    - Is the component mobile responsive with vertical stacking on sm breakpoint?
    - Did I miss any requirements?
    - Are there edge cases (e.g., price range validation, disabled states) I didn't handle?

    **Quality:**
    - Are component and variable names clear and accurate?
    - Is the code clean and maintainable?
    - Did I follow existing patterns in the codebase?

    **Discipline:**
    - Did I avoid overbuilding (YAGNI)?
    - Did I only build what was requested (no extra filter types, sorting, etc.)?
    - Did I avoid mocking the Zustand store?

    **Testing:**
    - Did I follow TDD? (test first, watch fail, minimal code, watch pass, refactor)
    - Would each test catch a real bug? If not, delete it.
    - Did I test all filter interactions (dropdown change, slider movement, checkbox toggle)?
    - Did I test the reset button?
    - Did I use the real productStore (not mocked)?
    - **REFERENCE:** See saurun:react-tdd for complete test quality criteria.

    **Tailwind v4:**
    - Did I use parentheses for CSS variables: `bg-(--var)` not `bg-[--var]`?
    - Did I use v4 utility names: `shadow-sm` not `shadow`, `rounded-sm` not `rounded`?
    - Did I use `cn()` from project utils for class merging?
    - Did I properly stack controls vertically on mobile (e.g., `flex flex-col sm:flex-row`)?

    If you find issues during self-review, fix them now before reporting.

    ## Report Format

    When done, report:
    - What you implemented (filter types, reset behavior, responsive layout)
    - What you tested and test results
    - Files changed
    - Self-review findings (if any)
    - Any issues or concerns
```

**Notes on this prompt:**

- **Full task text is pasted** — subagent doesn't need to read your plan file
- **Context explicitly covers** the ProductList dependency and store integration assumptions
- **Addresses common pitfalls** — reminds subagent to use real Zustand store, not mocked; includes Tailwind v4 specifics (parentheses for CSS vars, `cn()` usage, mobile-first naming)
- **Self-review is detailed** — tailored to this task (all filter types, reset button, mobile responsiveness)
- **TDD reinforcement** — links to `saurun:react-tdd` skill and emphasizes Red-Green-Refactor

Copy this prompt into your Task tool (or Skill tool if you're using `saurun:frontend-implementer`) and dispatch the subagent.
