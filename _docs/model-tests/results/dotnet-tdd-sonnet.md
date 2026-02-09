Complete TDD flow:

RED: Two tests written first (AddItem_ValidItemName_AddsItemToList, AddItem_InvalidItemName_ThrowsArgumentException with [Theory]). Fails because class doesn't exist.

GREEN: Minimal ShoppingList with _items list, AddItem with validation, Items as IReadOnlyList. Uses FluentAssertions. All tests pass.

REFACTOR: Code already clean, no refactoring needed.

Naming convention followed. Max 3 assertions. [Theory] for parameterized invalid names (empty, whitespace, null).
