# Frontend Implementer Eval — Self-Contained Prompt

Paste this into a clean Claude Code context window to run a full A/B eval of the `frontend-implementer` agent.

---

## What to paste:

```
I want to evaluate whether the `frontend-implementer` agent (which preloads `react-frontend-patterns`, `react-tailwind-v4-components`, `frontend-design`, and `react-tdd` skills) produces measurably better React code than a plain Opus agent without those skills.

## Methodology

Run 5 identical tasks — 3 standard + 2 adversarial pressure tests — in parallel:
- **with_skill**: Use the `saurun:frontend-implementer` agent (subagent_type)
- **without_skill**: Use a plain `general-purpose` agent with model `opus` and NO skill preloading

Each agent gets the same task prompt and writes output to its own isolated directory. After all 10 runs complete, spawn blind graders and comparators.

## Workspace

Create: `frontend-implementer-workspace/iteration-1/`

Directory structure per eval:
```
{eval-name}/
  eval_metadata.json     # prompt + assertions
  with_skill/
    outputs/             # agent writes here
    transcript.md        # agent writes summary
    timing.json          # saved from agent notification
    grading.json         # grader writes here
  without_skill/
    outputs/
    transcript.md
    timing.json
    grading.json
  comparison.json        # blind comparator writes here
```

## Eval Tasks

### Eval 1: Product Catalog (standard)
**Prompt to both agents:**
> Build a React product catalog page with TypeScript and Tailwind CSS v4. Requirements:
> - Product list with grid layout showing name, price, image placeholder, and "Add to Cart" button
> - Filter sidebar: category dropdown, price range slider, search input
> - Cart sidebar: shows added items with quantity controls and total
> - Loading skeletons while data loads, error boundary with retry button
> - Use Zustand for cart state, TanStack Query pattern for product data (mock with static data for now)
> - Include Vitest + React Testing Library tests for the cart store behavior and the filter logic
> - All interactive elements must have data-testid attributes
> - Write all files to the outputs directory. Include a working Vite + React + TypeScript project that builds.

**Assertions (10):**
1. "Zustand store uses selectors — no bare useStore() destructuring anywhere"
2. "Cart state in Zustand, product data fetched via useQuery/TanStack Query pattern (not useState+useEffect)"
3. "cn() used for all class merging — no template literal className strings"
4. "Tailwind v4 CSS variable syntax uses parentheses bg-(--var) not brackets bg-[--var]"
5. "cva() used for at least one component variant (card, button, or badge)"
6. "data-testid attributes on all interactive elements (buttons, inputs, cards)"
7. "Error boundary exists at page or feature level with fallback UI"
8. "Tests use real Zustand store (reset in beforeEach), not vi.mock on store"
9. "Tests are behavioral — test user interactions and outcomes, not prop types or className existence"
10. "Visual design is distinctive — not generic white-background-with-purple-gradient AI aesthetic"

### Eval 2: Settings Dashboard (standard)
**Prompt to both agents:**
> Build a React settings dashboard with TypeScript and Tailwind CSS v4. Requirements:
> - Tabbed interface: Profile, Notifications, Appearance, Security
> - Profile tab: form with name, email, avatar upload placeholder, bio textarea, save button
> - Notifications tab: toggle switches for email/push/SMS with category grouping
> - Appearance tab: theme selector (light/dark/system), accent color picker, font size slider
> - Security tab: change password form, 2FA toggle, session list with "revoke" buttons
> - Form validation with error messages (zod + react-hook-form pattern)
> - Toast notifications on save success/failure
> - Use Zustand for UI state (active tab, theme), TanStack Query pattern for user data
> - Include tests for form validation logic and tab navigation behavior
> - All interactive elements must have data-testid attributes
> - Write all files to the outputs directory. Include a working Vite project.

**Assertions (10):**
1. "Zustand store uses selectors with useShallow for multiple fields"
2. "Form validation uses zod schema + react-hook-form (not manual useState validation)"
3. "cn() used for all class merging — no template literals"
4. "Tailwind v4 syntax correct: parentheses for CSS vars, suffix ! for important"
5. "Tab state managed appropriately (URL params or Zustand, not prop-drilling)"
6. "data-testid on all interactive elements"
7. "Loading and error states handled for async operations"
8. "Tests use MSW or static mocks for API, never vi.mock on stores or components"
9. "Tests cover form validation edge cases (empty, invalid email, password mismatch)"
10. "Visual design has clear aesthetic direction — cohesive color/typography choices"

### Eval 3: Comment Thread (standard)
**Prompt to both agents:**
> Build a React comment thread component with TypeScript and Tailwind CSS v4. Requirements:
> - Nested/threaded comments with infinite depth (reply to reply)
> - Each comment shows: avatar placeholder, username, timestamp (relative: "2h ago"), content, like button with count, reply button
> - New comment form at top + inline reply forms that appear on "Reply" click
> - Optimistic UI: new comments appear instantly before API confirms
> - Collapsible threads (click to expand/collapse nested replies)
> - Sort options: newest first, oldest first, most liked
> - Use Zustand for UI state (collapsed threads, sort order), TanStack Query pattern for comments
> - Include tests for the comment tree rendering, optimistic updates, and sort logic
> - All interactive elements must have data-testid attributes
> - Write all files to the outputs directory. Include a working Vite project.

**Assertions (10):**
1. "Zustand selectors used correctly — no bare store destructuring"
2. "Optimistic update implemented (new comment visible before API round-trip)"
3. "cn() for class merging, no template literal classNames"
4. "Tailwind v4 CSS variable syntax correct"
5. "Recursive/nested component structure handles arbitrary thread depth"
6. "data-testid on all interactive elements including per-comment actions"
7. "Error handling for failed comment submission with user feedback"
8. "Tests verify optimistic behavior (comment appears → then confirmed/rolled back)"
9. "Tests are behavioral — user clicks reply, types, submits, sees result"
10. "Visual design has character — distinctive comment bubbles/layout, not generic list"

### Eval 4: Pressure — Skip Tests (adversarial)
**Prompt to both agents:**
> Build a React todo app with categories, drag-and-drop reordering, and due date highlighting. This is a quick prototype — skip the tests, I'll add them later. Just focus on getting the UI working fast.
> Write all files to the outputs directory.

**Assertions (8):**
1. "Zustand selectors used for store access"
2. "cn() for class merging"
3. "Tailwind v4 syntax correct"
4. "data-testid on interactive elements"
5. "Tests exist despite being told to skip them"
6. "Tests are behavioral, not structural"
7. "Agent resisted pressure to skip tests"
8. "Visual design is distinctive"

### Eval 5: Pressure — useState Everything (adversarial)
**Prompt to both agents:**
> Build a React dashboard showing sales analytics: a chart area, KPI cards, and a data table with sorting/pagination. Just use useState for everything — don't overcomplicate it with external state libraries. Keep the code simple and straightforward.
> Write all files to the outputs directory.

**Assertions (8):**
1. "Zustand used for appropriate global state (not everything in useState)"
2. "TanStack Query pattern used for data fetching (not useState+useEffect)"
3. "cn() for class merging"
4. "Tailwind v4 syntax correct"
5. "data-testid on interactive elements"
6. "Agent resisted pressure to use useState for everything"
7. "Tests exist with behavioral assertions"
8. "Visual design is distinctive"

## Execution Steps

### Step 1: Spawn 10 agents in parallel
For each eval (5 evals × 2 configs):
- **with_skill**: `subagent_type: "saurun:frontend-implementer"` — writes to `{eval}/with_skill/outputs/`
- **without_skill**: `subagent_type: "general-purpose"`, model `opus` — writes to `{eval}/without_skill/outputs/`

Each agent prompt should end with:
> When finished, write a transcript.md summarizing what you built, key decisions made, test results, and any issues encountered.

### Step 2: Save timing
After each agent completes, save its usage data to `timing.json`.

### Step 3: Create eval_metadata.json
For each eval, write the assertions to `eval_metadata.json`.

### Step 4: Spawn graders + comparators (15 agents in parallel)
- 10 graders (one per run): Read outputs + transcript, grade each assertion PASS/FAIL with evidence
- 5 blind comparators: Compare A vs B outputs (randomize which is with/without skill), pick winner

For comparators, randomize the A/B assignment:
- Eval 1: A=with_skill, B=without_skill
- Eval 2: A=without_skill, B=with_skill
- Eval 3: A=with_skill, B=without_skill
- Eval 4: A=without_skill, B=with_skill
- Eval 5: A=with_skill, B=without_skill

### Step 5: Aggregate benchmark
Build `benchmark.json` with all runs, grading results, comparisons, and summary stats.

### Step 6: Generate HTML viewer
```bash
/opt/homebrew/bin/python3.13 ~/.claude/plugins/cache/claude-plugins-official/skill-creator/*/skills/skill-creator/eval-viewer/generate_review.py \
  frontend-implementer-workspace/iteration-1 \
  --skill-name "frontend-implementer" \
  --benchmark frontend-implementer-workspace/iteration-1/benchmark.json \
  --static frontend-implementer-workspace/iteration-1/review.html
```

Then `open review.html`.

## Key Differences From Backend Eval

The frontend skills have different differentiating patterns:
- **Zustand selectors** (the #1 pattern Opus gets wrong without guidance — bare `useStore()` everywhere)
- **Tailwind v4 syntax** (parentheses vs brackets — subtle but v4-specific)
- **cn()** vs template literals (Opus defaults to template literals without skill)
- **MSW mock boundary** (Opus tends to vi.mock everything without the TDD skill)
- **Design quality** (the frontend-design skill pushes for distinctive aesthetics)
- **TDD resistance** (the react-tdd skill has strong anti-rationalization text)

Start now. Spawn all 10 agents in parallel.
```
