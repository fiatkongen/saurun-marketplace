5-task plan for shopping list add-item API:

Task 1: Test infrastructure (CustomWebApplicationFactory + TestHelpers) - correct.
Task 2: ShoppingItem value object. Implements reference, file paths, behaviors for name/quantity validation.
Task 3: ShoppingList aggregate. Implements reference, file paths, behaviors (create, add at <50, add at =50, ownership).
Task 4: EF Core DbContext + configuration. Implements reference, SQLite, owned Items.
Task 5: AddItem endpoint. Implements "POST /lists/{id}/items". Request + DTO files. Behaviors list individual validations: Name null/empty → 400, Name > 100 → 400, Quantity < 1 → 400, Quantity > 999 → 400, List at 50 → 400, Wrong user → 403.

Notes:
- Task 5 repeats validation rules inline instead of referencing architecture (skill says "Validates per Architecture §API Contract")
- Task 5 uses 403 for wrong-user instead of 404 (spec says "404 if list not found" — standard pattern is to return 404 for other user's list to avoid information leakage)
- Architecture reference says "Inline specification" instead of using placeholder path format
- Otherwise correct: Task 1 test infra, exact file paths, Implements references, proper dependencies, no full code
