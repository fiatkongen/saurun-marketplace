5-task plan for shopping list add-item API:

Task 1: Test infrastructure (CustomWebApplicationFactory + TestHelpers) - correct.
Task 2: ShoppingList + ShoppingItem domain entities. Implements reference, exact file paths, concise behaviors (create, add item, max capacity, invalid name, invalid quantity). Null quantity defaults to 1.
Task 3: EF Core DbContext + configuration. Implements reference, SQLite, owned Items. UserId indexed.
Task 4: ShoppingList repository. Interface + implementation. Exact paths, behaviors (GetByIdAsync returns/null, SaveChangesAsync).
Task 5: POST /lists/{id}/items endpoint. Implements reference. Request + DTO files. Behaviors: valid → 201, wrong user → 404, at capacity → 400, validates per architecture.

No full code. No repeated validation rules (references architecture). No 401/404 boilerplate. Dependencies correct. 3-6 task range (5 tasks). Concise behaviors.
