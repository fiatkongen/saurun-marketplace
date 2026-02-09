Complete TDD flow with two cycles:

CYCLE 1 - Empty Cart:
RED: Test renders CartPage, asserts 'Your cart is empty'. Fails (component doesn't exist).
GREEN: Minimal component returns <div>Your cart is empty</div>. Passes.
COMMIT.

CYCLE 2 - Items Exist:
RED: Test renders CartPage with items prop, asserts item names visible. Fails (still shows empty).
GREEN: Conditional rendering - empty message for no items, map items to divs. Both pass.
COMMIT.

Naming: it('should display empty message when cart has no items'). No vi.mock. Optional items prop. Minimal implementation.
