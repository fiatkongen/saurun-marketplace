Complete TDD flow with two cycles:

CYCLE 1 - Empty Cart:
RED: Test imports nonexistent CartPage, asserts 'Your cart is empty'. Fails.
GREEN: Minimal component returns empty message for items=[]. Passes.
COMMIT.

CYCLE 2 - Items Exist:
RED: Test renders CartPage with items, asserts item names visible. Fails (returns null for non-empty).
GREEN: Component renders <ul> of item names. Both tests pass.
COMMIT.

Naming: it('should show empty message when cart has no items'). No vi.mock. Props-based (items passed as prop). Minimal implementation at each step.
