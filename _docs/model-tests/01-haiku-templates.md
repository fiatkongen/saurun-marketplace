# Model Downgrade Tests: Opus → Haiku (Templates)

Test scenarios for validating whether Haiku can handle prompt template generation skills currently set to Opus.

## Test Methodology

## Critical Context Note

All skills in this file are **inline context injections** — their SKILL.md content is loaded into the model's context window before the task is given. The model reads the template/patterns and fills in blanks. It does NOT need to recall these from training.

**Both model runs (Opus baseline and Haiku proposed) MUST have the skill's SKILL.md content injected as context.** Without this, you're testing model knowledge, not instruction-following ability.

This makes Haiku MORE likely to succeed than naive testing suggests — the instructions are right there to follow.

1. Run each test input on current model (Opus)
2. Run same test input on proposed model (Haiku)
3. Compare outputs against GREEN/RED criteria
4. Document: Pass (Haiku sufficient) or Fail (needs bigger model)

---

### react-implementer-prompt (Opus → Haiku)

**What it does:** Generates fill-in dispatch prompts for React frontend implementation subagents with complete context, TDD workflow, and self-review checklist.

**Test input:**

```
I need to dispatch a subagent to implement Task 3 from my plan:

Task 3: Add Product Filter Component
- Create a ProductFilter component in components/products/
- Filter by category (dropdown), price range (dual slider), and availability (checkbox)
- Use Zustand productStore for filter state
- Apply filters on client side (filter products array)
- Reset button clears all filters
- Mobile responsive (stack vertically on sm)

Context: React 19 + Vite + TypeScript + Tailwind v4 + Zustand. Working directory is /Users/dev/shopping-app/frontend. This task depends on Task 1 (ProductList component) already being complete.

Generate the dispatch prompt for this task.
```

**GREEN criteria (pass — Haiku is sufficient):**

- Prompt includes all 7 required sections: Task Description, Context, Before You Begin, Your Job, TDD Workflow, Self-Review, Report Format
- Task Description section contains full task text (not a reference like "see task 3")
- Context section mentions dependencies (Task 1 ProductList)
- "Your Job" section references both required sub-skills: `saurun:react-tdd` and `saurun:react-tailwind-v4-components`
- Working directory is specified as absolute path: `/Users/dev/shopping-app/frontend`
- Self-review checklist includes all 5 categories: Completeness, Quality, Discipline, Testing, Tailwind v4
- Stack specification is correct: React 19 + Vite + TypeScript + Tailwind v4 + Zustand + Vitest + RTL + MSW
- TDD workflow section explicitly says "REQUIRED SUB-SKILL: Follow saurun:react-tdd"
- Report format lists all 5 expected outputs (what implemented, test results, files changed, self-review findings, issues/concerns)

**RED indicators (fail — needs bigger model):**

- Missing any of the 7 required prompt sections
- Task Description says "see task 3 in plan" instead of pasting full spec
- "Your Job" omits either `saurun:react-tdd` or `saurun:react-tailwind-v4-components` sub-skill
- Working directory is relative (e.g., `frontend/`) or missing
- Self-review checklist is abbreviated or missing categories (e.g., only "did I test?" without 5 categories)
- Includes instructions that weren't in the template (hallucinated extra steps)
- Fails to mention Task 1 dependency in Context section
- Stack specification is incomplete or wrong (e.g., omits MSW or says "React 18")

---

### dotnet-implementer-prompt (Opus → Haiku)

**What it does:** Generates dispatch prompts for .NET backend implementation subagents with complete context, TDD workflow, and self-review checklist.

**Test input:**

```
I need to dispatch a subagent to implement Task 2 from my plan:

Task 2: Add CreateOrder Command Handler
- Implement CreateOrderCommandHandler in Application/Orders/Commands/
- Command contains CustomerId, List<OrderItem> (ProductId, Quantity), ShippingAddress
- Validate: customer exists, all products exist, quantities > 0, max 100 items
- Create Order aggregate, add items, calculate total
- Save via IOrderRepository
- Return OrderId and Total
- Return 400 if validation fails, 404 if customer/product not found

Context: ShoppingApp.Backend, Orders bounded context, Application layer. Domain layer already has Order aggregate and OrderItem entity. Follow pattern from CreateCustomerCommandHandler. This is a Carter API project. Working directory is /Users/dev/shopping-app/backend. No dependencies on other tasks.

Generate the dispatch prompt.
```

**GREEN criteria (pass — Haiku is sufficient):**

- Prompt includes all 6 required sections from template: Task Description, Context, Before You Begin, Your Job, Self-Review, Report Format
- Task Description contains full task text (all 7 bullet points pasted verbatim)
- Context section mentions bounded context (Orders), layer (Application), existing patterns (CreateCustomerCommandHandler), and dependencies (Order aggregate exists)
- "Your Job" section has "REQUIRED SUB-SKILL: Follow `saurun:dotnet-tdd` strictly. No exceptions."
- Working directory is absolute path: `/Users/dev/shopping-app/backend`
- Self-review checklist has all 4 categories: Completeness, Quality, Discipline, Testing
- Testing category references `saurun:dotnet-tdd` verification checklist
- Verification command is `dotnet test`
- Report format lists all 5 items: what implemented, test results, files changed, self-review findings, issues/concerns

**RED indicators (fail — needs bigger model):**

- Missing any required section (Task Description, Context, Before You Begin, Your Job, Self-Review, Report Format)
- Task Description says "see task 2" instead of pasting full spec with all validation rules
- "Your Job" omits `saurun:dotnet-tdd` reference or doesn't call it "REQUIRED SUB-SKILL"
- Self-review checklist abbreviated to single paragraph instead of 4 categories
- Working directory is relative or missing
- Verification command is wrong (e.g., `npm test` instead of `dotnet test`)
- Adds extra instructions not in template (hallucinated steps like "write documentation")
- Fails to mention CreateCustomerCommandHandler pattern in Context
- Testing section doesn't reference `saurun:dotnet-tdd` verification checklist

---

### dotnet-code-quality-reviewer-prompt (Opus → Haiku)

**What it does:** Generates ADDITIONAL_REVIEW_CRITERIA for dispatching .NET code quality review subagents, focusing on xUnit test quality (behavioral tests, assertion counts, NSubstitute mock boundaries, anti-patterns).

**Test input:**

```
I need to dispatch a code review subagent for the CreateOrder feature that just got implemented.

Implementation report summary:
- Added CreateOrderCommandHandler with validation (customer exists, products exist, quantities > 0, max 100 items)
- Added xUnit tests: CreateOrderCommandHandlerTests (12 tests covering happy path, validation failures, 404 cases)
- Files changed: CreateOrderCommand.cs, CreateOrderCommandHandler.cs, CreateOrderCommandHandlerTests.cs, CreateOrderEndpoint.cs

Plan: Task 2 from _docs/plans/orders-feature.md

Commits:
- Base SHA (before implementation): abc123def
- HEAD SHA (current): 456ghi789

Generate the dispatch prompt for the code-reviewer agent with .NET-specific quality criteria.
```

**GREEN criteria (pass — Haiku is sufficient):**

- Output is structured as Task tool call targeting `superpowers:code-reviewer` agent
- Includes all 5 placeholder variables: WHAT_WAS_IMPLEMENTED, PLAN_OR_REQUIREMENTS, BASE_SHA, HEAD_SHA, DESCRIPTION
- WHAT_WAS_IMPLEMENTED contains the implementation summary from test input
- PLAN_OR_REQUIREMENTS references "Task 2 from _docs/plans/orders-feature.md"
- BASE_SHA and HEAD_SHA use actual values from test input (abc123def and 456ghi789)
- ADDITIONAL_REVIEW_CRITERIA section includes all 8 checklist categories: Behavioral Testing, Test Structure, Test Infrastructure, Mock Boundaries, EF Core Patterns, Coverage, Anti-Patterns to Flag, Severity Guide
- Behavioral Testing checklist includes "No getter/setter tests" flag with examples (CanSetProperties, CanSetName, IsNullable, etc.)
- Mock Boundaries mentions NSubstitute specifically
- Anti-Patterns section lists at least 8 named anti-patterns (CanSetProperties, MockVerifyOnly, DomainMock, AssertionOverload, etc.)
- Severity Guide has 3 levels: Critical, Important, Minor

**RED indicators (fail — needs bigger model):**

- Missing any of the 5 placeholder variables (WHAT_WAS_IMPLEMENTED, PLAN_OR_REQUIREMENTS, BASE_SHA, HEAD_SHA, DESCRIPTION)
- ADDITIONAL_REVIEW_CRITERIA is abbreviated (e.g., only 3-4 categories instead of 8)
- Behavioral Testing section doesn't flag getter/setter tests or provide examples
- Anti-Patterns section lists fewer than 10 patterns
- Severity Guide missing or doesn't have 3 distinct levels
- Doesn't target `superpowers:code-reviewer` agent
- BASE_SHA/HEAD_SHA left as placeholders instead of using abc123def/456ghi789 from input
- EF Core Patterns section missing or empty
- Test Infrastructure section doesn't mention CustomWebApplicationFactory or SQLite
- Adds unrelated criteria not in template (e.g., "check for security vulnerabilities")

---

### react-code-quality-reviewer-prompt (Opus → Haiku)

**What it does:** Generates ADDITIONAL_REVIEW_CRITERIA for dispatching React code quality review subagents, focusing on test quality (behavioral tests, assertion counts, mock boundaries, anti-patterns like className tests).

**Test input:**

```
I need to dispatch a code review subagent for the ProductFilter component that just got implemented.

Implementation report summary:
- Added ProductFilter component with category dropdown, price range slider, availability checkbox
- Connected to Zustand productStore for filter state
- Added ProductFilter.test.tsx with 15 tests (filter application, reset, mobile responsive, accessibility)
- Files changed: ProductFilter.tsx, ProductFilter.test.tsx, productStore.ts

Plan: Task 3 from _docs/plans/products-feature.md

Commits:
- Base SHA: aaa111bbb
- HEAD SHA: ccc222ddd

Generate the dispatch prompt for the code-reviewer with React-specific quality criteria.
```

**GREEN criteria (pass — Haiku is sufficient):**

- Output is structured as Task tool call targeting `superpowers:code-reviewer` agent
- Includes all 5 placeholder variables: WHAT_WAS_IMPLEMENTED, PLAN_OR_REQUIREMENTS, BASE_SHA, HEAD_SHA, DESCRIPTION
- WHAT_WAS_IMPLEMENTED contains the implementation summary from test input
- PLAN_OR_REQUIREMENTS references "Task 3 from _docs/plans/products-feature.md"
- BASE_SHA and HEAD_SHA use actual values (aaa111bbb and ccc222ddd)
- ADDITIONAL_REVIEW_CRITERIA includes all 11 checklist categories: Behavioral Testing, Test Structure, Test Infrastructure, Mock Boundaries, Coverage, React 19 Specifics, Tailwind v4 Compliance, Zustand Store Review, Accessibility, Anti-Patterns to Flag, Severity Guide
- Behavioral Testing section flags "No className/style tests" with examples (toHaveClass, toHaveStyle)
- Mock Boundaries mentions "MSW for HTTP only, real Zustand stores"
- Anti-Patterns section lists at least 10 of 11 named patterns (CanSetProperties, ClassNameTest, RendersSmokeTest, MockVerifyOnly, AssertionOverload, DualBehavior, StoreMock, Assertionless, DirectSetState, ProviderDuplicate, V3Syntax)
- Tailwind v4 section checks for v3→v4 syntax issues (`[--var]` → `(--var)`, renamed utilities)
- Severity Guide has 3 levels: Critical, Important, Minor

**RED indicators (fail — needs bigger model):**

- Missing any placeholder variables or leaving them as literal placeholders instead of using test input values
- ADDITIONAL_REVIEW_CRITERIA abbreviated to fewer than 9 categories
- Behavioral Testing doesn't flag className tests
- Mock Boundaries doesn't mention MSW or says to mock Zustand stores
- Anti-Patterns section lists fewer than 10 patterns
- Tailwind v4 Compliance section missing or doesn't check `[--var]` → `(--var)` syntax
- React 19 Specifics section missing entirely
- Accessibility section missing or empty
- Doesn't target `superpowers:code-reviewer` agent
- Adds unrelated criteria (e.g., "performance benchmarks", "SEO checks")
- Zustand Store Review section missing

---

### react-tailwind-v4-components (Opus → Haiku)

**What it does:** Reference guide for building React components with Tailwind CSS v4 syntax (CSS variables with parentheses, new utility sizes, component patterns).

**Test input:**

```
I'm building an Alert component with variants (info, success, warning, error). Each variant should have a different accent color using CSS variables. The component should:
- Use cva for variant management
- Use parentheses syntax for CSS variables
- Follow the "data-attribute + CSS" pattern for setting variant-specific CSS variables
- Have proper focus states with outline-offset
- Use the smallest shadow size

Show me the implementation.
```

**GREEN criteria (pass — Haiku is sufficient):**

- Component uses `cva` from class-variance-authority with `variants` object
- CSS variable syntax uses parentheses: `bg-(--alert-accent)` NOT `bg-[--alert-accent]` or `bg-[var(--alert-accent)]`
- Implements data-attribute pattern: component has `data-variant={variant}` attribute
- CSS file defines variant-specific CSS variables: `[data-variant="info"] { --alert-accent: var(--color-blue-500); }`
- Shadow uses `-xs` suffix: `shadow-xs` (smallest size from scale)
- Focus state uses `outline-offset-*` NOT deprecated `ring-offset-*`
- Uses `cn()` utility for class merging: `cn(alertVariants({ variant }), className)`
- Imports `cn` from `@/lib/utils` or similar path
- No template literals for className composition (`` `base ${x}` `` is wrong)

**RED indicators (fail — needs bigger model):**

- CSS variable syntax uses brackets: `bg-[--alert-accent]` or `bg-[var(--alert-accent)]`
- Uses inline style pattern (`style={{ "--alert-accent": ... }}`) when data-attribute pattern was specifically requested
- Shadow uses wrong size: `shadow-sm` or `shadow` instead of `shadow-xs`
- Focus state uses deprecated `ring-offset-*` instead of `outline-offset-*`
- Uses template literals for class merging instead of `cn()`: `` className={`base ${x}`} ``
- Omits `data-variant` attribute
- Doesn't provide CSS file with `[data-variant="..."]` selectors
- Uses `theme(colors.blue.500)` instead of `var(--color-blue-500)`
- Component doesn't use `cva` at all
- Uses v3 important syntax: `!text-red-500` instead of `text-red-500!`
- Hallucinated CSS variable names not in test input (e.g., `--tw-alert-color` instead of `--alert-accent`)

---

## Summary Table

| Skill | Type | Key Failure Modes |
|-------|------|-------------------|
| react-implementer-prompt | Template Generator | Missing sections, abbreviated checklists, wrong sub-skill refs |
| dotnet-implementer-prompt | Template Generator | Missing sections, wrong test command, omitted TDD reference |
| dotnet-code-quality-reviewer-prompt | Criteria Generator | Abbreviated categories, missing anti-patterns, placeholder vars not filled |
| react-code-quality-reviewer-prompt | Criteria Generator | Abbreviated categories, missing Tailwind v4 checks, wrong mock boundaries |
| react-tailwind-v4-components | Code Generator | Wrong CSS var syntax, deprecated utilities, template literals instead of cn() |

## Testing Notes

- **Template generators** (react/dotnet-implementer-prompt): Primary risk is omitting required sections or abbreviating checklists. Haiku may compress multi-section prompts into shorter forms.
- **Criteria generators** (code-quality-reviewer-prompts): Risk is losing detail in the 8-9 category checklists. Haiku may consolidate categories or drop anti-pattern lists.
- **Code generator** (react-tailwind-v4-components): Risk is syntax mistakes (brackets vs parentheses for CSS vars) and using deprecated patterns. Haiku may not retain v3→v4 migration details.

## Next Steps

1. Run each test scenario on Opus and capture full output
2. Switch skill model to Haiku and run same scenarios
3. Score outputs against GREEN/RED criteria
4. Document failures with specific examples
5. Decide: keep Opus or accept Haiku with known limitations
