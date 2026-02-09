Complete TDD flow:

RED: Three tests written first (AddItem_ValidName_AddsItemToList, AddItem_EmptyOrNullName_ThrowsArgumentException with [Theory], AddItem_MultipleCalls_AddsAllItems). Fails because classes don't exist.

GREEN: Minimal ShoppingList + ShoppingListItem. Validation in AddItem. Real domain objects, no mocks. All tests pass.

REFACTOR: Moved validation into ShoppingListItem constructor (invariant at value object boundary). ShoppingList.AddItem becomes thinner. All tests still green.

Naming convention followed. Max 3 assertions. [Theory] for parameterized invalid names.
