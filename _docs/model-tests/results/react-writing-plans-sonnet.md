4-task plan for recipe submission form:

Task 1: Test infrastructure (renderWithProviders + MSW + Zustand reset) - correct files (test-utils.tsx, mocks/server.ts, mocks/handlers.ts, vitest.setup.ts).
Task 2: IngredientInput component. Implements reference ("IngredientInput - dynamic ingredient field"). File paths with __tests__. Behaviors: Name required, Amount required, Add button, Remove button (disabled when 1). Max 4 behaviors.
Task 3: StepInput component. Implements reference. File paths. Behaviors: Step text required, add, remove (disabled when 1), maintains order. Max 4 behaviors.
Task 4: RecipeForm component. Implements reference ("RecipeForm - main form with validation"). Behaviors grouped by field:
- Title: required, 1-200 chars
- Description: required, 1-2000 chars
- Servings: required, 1-100
- PrepTime: required, 0-1440 min
- Ingredients: min 1 item (validated by IngredientInput)
- Steps: min 1 item (validated by StepInput)
- Valid submit calls onSubmit with CreateRecipeRequest
- Invalid submit shows field errors
Dependencies: Task 2-3.

8 behaviors = 6 fields + 2 submit = correct per hard limit. No CSS, no implementation details, no summary tables. Architecture reference notes "inline specification" instead of placeholder. Missing announcement ("I'm using the react-writing-plans skill...").
