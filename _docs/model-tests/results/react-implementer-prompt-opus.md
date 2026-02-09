Here is the dispatch prompt for Task 3:

---

```
Task tool (saurun:frontend-implementer):
  description: "Implement Task 3: Add Product Filter Component"
  prompt: |
    You are implementing Task 3: Add Product Filter Component

    ## Task Description

    - Create a ProductFilter component in components/products/
    - Filter by category (dropdown), price range (dual slider), and availability (checkbox)
    - Use Zustand productStore for filter state
    - Apply filters on client side (filter products array)
    - Reset button clears all filters
    - Mobile responsive (stack vertically on sm)

    ## Context

    This is a client-side filtering component for the product listing page. Task 1 (ProductList component) is already complete — ProductFilter will work alongside it. The productStore (Zustand) already exists and holds the products array. Your job is to add filter state to that store (categories, price range, availability) and create the UI component that reads/writes that state.

    The filtered products should be derived from the store (a selector or computed value that applies active filters to the full products array). ProductList already renders products from the store — once you add filtered products, ProductList should consume the filtered version.

    ## Before You Begin

    If you have questions about:
    - The requirements or acceptance criteria
    - The approach or implementation strategy
    - Dependencies or assumptions
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

    ## Implementation Notes

    **Store changes:** Add filter state to the existing productStore. Do NOT create a separate filter store. The filter state shape should include:
    - `selectedCategory: string | null`
    - `priceRange: { min: number; max: number }`
    - `availableOnly: boolean`
    - Actions: `setCategory`, `setPriceRange`, `setAvailableOnly`, `resetFilters`
    - A selector: `useFilteredProducts` that derives the filtered array from products + filter state

    **Component structure:**
    - Category dropdown: renders categories derived from the products array (unique values)
    - Price range: dual slider (min/max). Use native range inputs or existing UI primitives if available in the project.
    - Availability: single checkbox "In stock only"
    - Reset button: clears all filters back to defaults

    **Responsive:** Filters lay out horizontally on md+ screens, stack vertically on sm. Use Tailwind v4 responsive utilities (`flex flex-col sm:flex-row` or similar).

    **Dependency on Task 1:** ProductList already exists and renders from the store. After your changes, ProductList should use `useFilteredProducts` instead of the raw products array. Update that import/usage.

    ## TDD Workflow

    **REQUIRED SUB-SKILL:** Follow saurun:react-tdd for the full Red-Green-Refactor cycle. No shortcuts, no skipping steps.

    Expected RED steps (in rough order):
    1. Test that ProductFilter renders category dropdown, price sliders, availability checkbox, and reset button
    2. Test that selecting a category filters the product list (render ProductFilter + ProductList together, assert filtered results)
    3. Test that adjusting price range filters products outside range
    4. Test that toggling availability hides out-of-stock products
    5. Test that reset button clears all filters and shows all products
    6. Test responsive layout classes are applied

    Use real Zustand store — do NOT mock the store. Use MSW only if API calls are involved (they shouldn't be for client-side filtering, but if the store fetches products, mock that fetch with MSW).

    ## Before Reporting Back: Self-Review

    Review your work with fresh eyes. Ask yourself:

    **Completeness:**
    - Did I fully implement everything in the spec?
    - Did I miss any requirements?
    - Are there edge cases I didn't handle? (empty products, no matches, all filters active simultaneously)

    **Quality:**
    - Is this my best work?
    - Are names clear and accurate?
    - Is the code clean and maintainable?

    **Discipline:**
    - Did I avoid overbuilding (YAGNI)?
    - Did I only build what was requested?
    - Did I follow existing patterns in the codebase?

    **Testing:**
    - Did I follow TDD? (test first, watch fail, minimal code, watch pass, refactor)
    - Would each test catch a real bug? If not, delete it.
    - Did I use real Zustand stores (not mocked)?
    - Did I use MSW for API boundaries?
    - **REFERENCE:** See saurun:react-tdd for complete test quality criteria.

    **Tailwind v4:**
    - Did I use parentheses for CSS variables: `bg-(--var)` not `bg-[--var]`?
    - Did I use v4 utility names: `shadow-sm` not `shadow`, `rounded-sm` not `rounded`?
    - Did I use `cn()` from project utils (typically `lib/utils.ts` in shadcn/ui projects) for class merging?

    If you find issues during self-review, fix them now before reporting.

    ## Report Format

    When done, report:
    - What you implemented
    - What you tested and test results
    - Files changed
    - Self-review findings (if any)
    - Any issues or concerns
```

---

**What this prompt does:**

- Pastes the full task spec inline so the subagent never needs to read a plan file
- Sets architectural context: productStore already exists, ProductList (Task 1) is complete, filters are client-side
- Prescribes the store shape (filter state, actions, selector) so the subagent doesn't have to guess
- Specifies that ProductList must be updated to use `useFilteredProducts`
- Lists expected RED test steps so the subagent can verify TDD is on track
- Calls out the real Zustand store requirement (no mocking the store)
- Includes responsive layout direction (horizontal on md+, stacked on sm)
- References both required sub-skills (`react-tdd`, `react-tailwind-v4-components`)
