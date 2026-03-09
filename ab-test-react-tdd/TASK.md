# A/B Test: react-tdd skill

## Task (identical for both variants)

Build a **Shopping Cart** feature in the React+Vite+TypeScript project with tests.

### Requirements
1. **Zustand store** (`useCartStore`) with: `items`, `addItem`, `removeItem`, `updateQuantity`, `getTotal`
2. **CartPage component** showing:
   - Empty state message when no items
   - List of cart items with name, price, quantity
   - Remove button per item
   - Quantity +/- controls
   - Cart total at bottom
3. **ProductCard component** with "Add to Cart" button that adds item to store
4. **MSW mock** for `GET /api/products` returning a product list
5. **Tests** covering all user-visible behaviors

### Constraints
- Use Vitest + React Testing Library + MSW
- Use Zustand for state
- Write tests and implementation

## Scoring Criteria (0-5 each)

| Criterion | What to check |
|-----------|---------------|
| TDD discipline | Did tests come before implementation? (check git log / file order) |
| Mock boundary | Only MSW for API? No vi.mock on stores/components? |
| Behavioral tests | Tests assert on user-visible outcomes, not internals? |
| Assertion quality | Max 3 per test? it.each for parameterized? No assertion-less tests? |
| Test naming | `should [behavior] when [condition]` pattern? |
| Custom render | Shared test-utils wrapper? No duplicate provider setup? |
| Anti-patterns | No className testing, no renders-without-crashing, no test-only props? |
| Test coverage | All 5 requirements covered by tests? |
| Code quality | Clean, minimal implementation? |
| Tests pass | Do all tests actually pass? |
