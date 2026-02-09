4-task plan for recipe submission form:

Task 1: Test infrastructure (renderWithProviders + MSW + Zustand reset) - correct files (test-utils.tsx, mocks/server.ts, mocks/handlers.ts, vitest.setup.ts).
Task 2: IngredientInput component. Implements reference. Exact file paths with __tests__. Behaviors: name/amount fields, add button, remove button, cannot remove when only 1. Max 4 behaviors.
Task 3: StepInput component. Implements reference. Exact paths. Behaviors: textarea per step, add, remove, cannot remove when only 1. Max 4 behaviors.
Task 4: RecipeForm component. Implements reference. Exact paths. Behaviors grouped by field:
- Title: required, 1-200 chars
- Description: required, 1-2000 chars
- Servings: required, 1-100
- PrepTime: required, 0-1440 min
- Ingredients: min 1 item (validated by IngredientInput)
- Steps: min 1 item (validated by StepInput)
- Valid submit calls onSubmit with CreateRecipeRequest DTO
- Invalid submit shows field-level errors, does not call onSubmit
Dependencies: Task 2, Task 3.

8 behaviors for form = 6 fields + 2 submit = correct per hard limit (field count + 2-3). No CSS, no implementation details, no summary tables. Announcement present.
