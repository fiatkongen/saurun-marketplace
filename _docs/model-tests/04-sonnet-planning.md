# Model Downgrade Test: Planning Skills (Opus → Sonnet)

## Critical Context Note

All skills in this file are **inline knowledge injections** — their SKILL.md content is loaded into the model's context window. The model reads the patterns/rules and applies them to the task. It does NOT need to know DDD or React patterns from training data.

**Both model runs (Opus baseline and Sonnet proposed) MUST have the skill's SKILL.md content injected as context.** This is testing instruction-following ability, not domain knowledge.

**Methodology:** Run each scenario twice (Opus baseline, Sonnet proposed) with the skill content in context. Compare outputs against GREEN/RED criteria.

---

### dotnet-writing-plans (Opus → Sonnet)

**What it does:** Creates concise .NET implementation plans with task dependencies, contract references, and behavior lists.

**Test input:**
```
Create an implementation plan for this feature:

Goal: Build a shopping list API endpoint to add items
Architecture: Shopping lists belong to users. Each list can have max 50 items. Items have name (required, 1-100 chars) and quantity (optional, 1-999). API should validate ownership, return 201 on success, 400 for validation errors, 404 if list not found.
```

**GREEN criteria (pass — Sonnet is sufficient):**
- Creates Task 1 for test infrastructure (CustomWebApplicationFactory + TestHelpers)
- All tasks have exact file paths (Create/Modify/Test)
- Each task has `Implements:` contract reference
- Behaviors are concise (1 line each), not verbose
- Dependencies properly identified (e.g., Task 5 depends on Task 2)
- Does NOT repeat validation rules inline (references architecture instead)
- Does NOT include full test code or implementation code
- Task count appropriate (3-6 tasks, not 10+)

**RED indicators (fail — needs bigger model):**
- Missing test infrastructure in Task 1
- Vague behaviors like "handles errors" instead of "Empty name → 400"
- No `Implements:` contract references
- Tasks list full test code or implementation code
- Repeats validation rules instead of referencing architecture
- File paths missing or incorrect (relative paths, typos)
- Circular dependencies or missing dependency tracking
- Tasks too large (>3 files per task)

---

### react-writing-plans (Opus → Sonnet)

**What it does:** Creates concise React implementation plans with form validation grouped by field and strict conciseness rules.

**Test input:**
```
Create an implementation plan for this feature:

Goal: Build a recipe submission form
Architecture: RecipeForm component with fields: title (required, 1-200 chars), description (required, 1-2000 chars), servings (required, 1-100), prepTime (required, 0-1440 min), ingredients (min 1, array of {name, amount}), steps (min 1, array of strings). Form should show field-level validation errors and call onSubmit with CreateRecipeRequest DTO on valid submit. Uses React 19, TypeScript, Tailwind v4, Vitest, React Testing Library.
```

**GREEN criteria (pass — Sonnet is sufficient):**
- Creates Task 1 for test infrastructure (renderWithProviders + MSW setup)
- Form behaviors grouped by field (1 line per field with all constraints)
- Example: "Title: required, 1-200 chars" NOT split into multiple behaviors
- Does NOT list CSS/styling details ("applies flex layout")
- Does NOT list standard patterns ("accepts className", "disables when loading")
- Max ~7-9 behaviors for form (field count + 2-3 for submit/populate)
- All tasks have `Implements:` references to architecture
- File paths use exact component structure (Create + Test)

**RED indicators (fail — needs bigger model):**
- Splits field validation into separate behaviors ("Title required", "Title max 200")
- Lists implementation details as behaviors ("Applies centered flex", "Shows spinner")
- Includes full test code or component implementation
- No grouping by field for validation
- Missing test infrastructure in Task 1
- Vague behaviors instead of concrete constraints
- Tasks too verbose (>10 behaviors for simple form)
- Forgets Tailwind v4 syntax rules (uses `bg-[--var]` instead of `bg-(--var)`)

---

> **⛔ EXEMPTED FROM TESTING** — Staying on Opus. Linguistic nuance (Danish-conjugated anglicisms like "performer", "onboardede", English words with Danish suffixes like "UI'et", "scopet") requires native-level language understanding that Sonnet may not match.

### ~~dansk-qa~~ (Opus → Sonnet) — EXEMPTED

**What it does:** Scans Danish text for 6 types of linguistic issues (anglicisms, tech jargon, defensive tone, vague formulations, English words, formal language).

**Test input:**
```
Scan this Danish text for issues:

"Vores platform performer rigtig godt efter vi onboardede de nye enterprise-kunder. Vi har deployet den seneste version til produktion og monitorerer KPI'erne tæt.

Teamet brainstormede løsninger til den outdatede UX, og vi har nu et roadmap for Q3. Vi kan desværre ikke garantere at alle edge cases er håndteret, men vi arbejder proaktivt på at levere en skalerbar og robust løsning der matcher jeres forventninger.

Kontakt os gerne for at schedule et opfølgende møte — vi pitcher gerne vores approach til stakeholderne."
```

**Expected findings by category:**

| Category | Issues |
|----------|--------|
| **Anglicisms** | "performer" → "klarer sig", "onboardede" → "fik ombord", "monitorerer" → "overvåger", "brainstormede" → "udviklede idéer", "pitcher" → "præsenterer" |
| **Tech jargon** | "deployet" → "sat i drift", "edge cases" → "særtilfælde", "skalerbar" → "kan vokse med jer", "KPI'erne" → "nøgletallene" |
| **Defensive tone** | "Vi kan desværre ikke garantere" → reframe positively ("Vi arbejder målrettet på...") |
| **Vague formulations** | "performer rigtig godt" → concrete metrics, "proaktivt" → specific actions, "robust løsning" → what makes it robust |
| **English words** | "enterprise-kunder" → "erhvervskunder", "roadmap" → "plan/køreplan", "schedule" → "aftale/planlægge", "approach" → "tilgang", "stakeholderne" → "interessenterne", "UX" → "brugeroplevelsen" |
| **Formal language** | "møte" (this is actually Norwegian, flag as non-Danish) |

**Key difficulty:** Many anglicisms have Danish conjugation ("performer", "onboardede", "deployet", "brainstormede", "pitcher") — these are harder to catch than obvious English words like "DONE" or "features".

**GREEN criteria (pass — Sonnet is sufficient):**
- Detects ALL 5 Danish-conjugated anglicisms (performer, onboardede, monitorerer, brainstormede, pitcher)
- Detects tech jargon even when Danish-ified (deployet, KPI'erne)
- Catches defensive tone pattern ("kan desværre ikke garantere")
- Flags vague buzzwords (proaktivt, robust, skalerbar)
- Catches English words with Danish articles/suffixes (enterprise-kunder, KPI'erne, stakeholderne)
- Provides idiomatic Danish alternatives (not literal translations)
- Flags "møte" as Norwegian (not Danish — should be "møde")
- Correctly categorizes each issue (anglicism vs tech jargon vs English word)

**RED indicators (fail — needs bigger model):**
- Misses Danish-conjugated anglicisms (treats "performer" or "pitcher" as Danish)
- Misses 3+ English words with Danish suffixes
- Provides non-idiomatic replacements ("enterprise-kunder" → "virksomheds-kunder" instead of "erhvervskunder")
- Doesn't distinguish categories (lumps everything as "anglicisms")
- Misses defensive tone entirely
- Accepts tech jargon because "it's industry standard"
- Doesn't catch "møte" as Norwegian
- Fewer than 15 total issues found (text has 20+)

---

### ~~autonomous-task-executor~~ (Opus → Sonnet) — REMOVED

> **🚫 REMOVED** — The `autonomous-task-executor` skill does not exist in `plugins/saurun/skills/`. No SKILL.md to inject as context, making this test impossible to run. If the skill is added to the repo in the future, recreate this test case.
