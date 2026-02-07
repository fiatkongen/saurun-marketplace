# Completeness Dimensions

7 dimensions for scoring input completeness. Used in Step 3 to determine interview depth.

## Dimension Table

| # | Dimension | What to look for | Full credit example | No credit example |
|---|-----------|-----------------|--------------------|--------------------|
| 1 | Problem | Specific pain point | "Families can't communicate cleaning tasks to foreign-language cleaners" | "Build a cleaning app" |
| 2 | Target Users | Named user types with context | "Homeowners (manage tasks) + cleaning staff (view/complete)" | "Users" |
| 3 | Core Features | 3+ specific capabilities | "Task templates with photos, voice instructions, real-time progress" | "Standard features" |
| 4 | Entities | Named domain objects | "Household, CleaningTemplate, Task, Cleaner, Rating" | No entities mentioned |
| 5 | User Flows | Step-by-step journeys | "Create template -> add tasks with photos -> assign cleaner -> cleaner opens link -> views tasks" | "Users can do things" |
| 6 | Constraints | Technical/business limits | "Must work offline, Danish market, GDPR, mobile-first" | No constraints |
| 7 | Visual/UX | Style/feel preferences | "Warm, friendly, Scandinavian minimal, large touch targets" | No preferences |

## Scoring Algorithm

```
COMPLETENESS_SCORE = 0
For each of 7 dimensions:
  Read user input + any attached spec/PRD
  Ask: "Could an implementer make a specific decision from this
        detail without asking follow-ups?"
  YES -> +1 point (dimension is covered)
  NO  -> +0 points (dimension needs interview)
```

## Score Examples

- `"Build a recipe app"` -> **0** (nothing specific)
- `"Build a recipe app for Danish families with kids, mobile-first"` -> **2** (Target Users: partial but actionable, Constraints: mobile-first is specific)
- `"Build a recipe app"` + attached PRD with problem, users, 8 features, entity list, 3 user flows, "GDPR compliant" -> **6**
- Full PRD with all 7 dimensions filled in detail -> **7**

## Interview Depth Mapping

| Score | Depth | Rounds | Strategy |
|-------|-------|--------|----------|
| 0-2 | FULL | 5-8 | Most info missing, deep discovery needed. Cover all uncovered dimensions plus edge cases, failure modes, scale. |
| 3-5 | LIGHT | 2-3 | Some gaps. Focus only on uncovered dimensions, clarify ambiguities. |
| 6-7 | VALIDATION | 1 | Comprehensive input. Quick confirmation: "Here's my understanding -- anything wrong?" |
