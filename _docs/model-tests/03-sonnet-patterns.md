# Model Downgrade Test Scenarios: Opus → Sonnet (Pattern Skills)

## Critical Context Note

All skills in this file are **inline knowledge injections** — their SKILL.md content is loaded into the model's context window. The model reads the patterns/rules and applies them to the task. It does NOT need to know DDD or React patterns from training data.

**Both model runs (Opus baseline and Sonnet proposed) MUST have the skill's SKILL.md content injected as context.** This is testing instruction-following ability, not domain knowledge.

**Methodology:** Run each scenario twice (Opus baseline, Sonnet proposed) with the skill content in context. Compare outputs against GREEN/RED criteria.

---

### dotnet-tactical-ddd (Opus → Sonnet)

**What it does:** Enforces tactical DDD patterns (value objects, aggregates, repositories, Result<T>, DTOs) in .NET backend development.

**Test input:**
"Create a Product entity with a Price property. Add a method to update the price. Expose it via a controller endpoint."

**GREEN criteria (pass — Sonnet is sufficient):**
- Product has private setter on Price property
- UpdatePrice() method returns Result<T> with validation
- Private constructor + static Create() factory method with validation
- Price is a Money value object (not decimal), immutable with validation
- Controller uses ProductDto (not Product entity in return type)
- Extension methods for ToDto() and UpdateFromRequest() mapping (no AutoMapper, no inline mapping)
- EF Core configuration for owned Money value object with OwnsOne()
- No dependency violations (Domain doesn't reference Infrastructure)

**RED indicators (fail — needs bigger model):**
- Public setters on Product entity
- Raw decimal instead of Money value object
- Product entity exposed directly in controller (no DTO)
- Missing Result<T> pattern, throws exceptions instead
- Inline mapping in controller instead of extension methods
- No factory method (public constructor allows invalid state)
- Missing EF Core configuration for owned types
- AutoMapper used despite explicit prohibition
- Domain layer importing EF Core or HTTP libraries

---

### react-frontend-patterns (Opus → Sonnet)

**What it does:** Enforces React 19 + Zustand + TanStack Query + Tailwind v4 architecture patterns (state colocation, selector usage, API caching).

**Test input:**
"Create a shopping cart feature with a list of items, add/remove functionality, and total calculation. Fetch product data from /api/products."

**GREEN criteria (pass — Sonnet is sufficient):**
- Zustand store uses selectors everywhere: `useCartStore((s) => s.items)` — never bare `useCartStore()`
- Multiple fields use useShallow: `useCartStore(useShallow((s) => ({ items: s.items, total: s.total })))`
- Export selector hooks (useCartItems, useCartTotal, useCartActions)
- Derived state (total) computed in selector, not stored in state
- Product data fetched via TanStack Query useQuery hook (not Zustand, not useEffect + useState)
- Query keys follow convention: `['products', 'list', { category }]`
- Error boundaries at page level
- All interactive elements have data-testid attributes with correct naming: `cart-item-{id}`, `cart-remove-button`

**RED indicators (fail — needs bigger model):**
- Bare store usage: `const { items, total } = useCartStore()` without selectors
- Storing derived state instead of computing in selector
- API data stored in Zustand instead of TanStack Query
- Missing useShallow for multiple field selection
- useEffect + useState for data fetching instead of useQuery
- Missing or incorrectly formatted data-testid attributes
- No error boundaries
- Query keys missing parameters or not following convention
- Storing itemCount when items array exists (should derive: `(s) => s.items.length`)

---

### dotnet-tdd (Opus → Sonnet)

**What it does:** Enforces test-first development for .NET (xUnit + NSubstitute) — write failing test before implementation.

**Test input:**
"Implement a ShoppingList.AddItem() method that validates item name is not empty and adds the item to the list."

**GREEN criteria (pass — Sonnet is sufficient):**
- Test written FIRST before any implementation code
- Test named using convention: `AddItem_WithValidName_AddsItemToList`
- Test uses real domain objects (ShoppingItem, ShoppingList) — no mocks for domain
- NSubstitute used for infrastructure mocks (if any), not Moq
- Max 3 assertions per test or uses [Theory] for multiple cases
- Test verified to fail correctly (missing method) before implementation
- Minimal implementation to pass the test (no extra features)
- No "CanSetProperty" or getter/setter tests
- Integration test includes actual DB/response assertions, not just HTTP status code

**RED indicators (fail — needs bigger model):**
- Implementation code written before test
- Test passes immediately (never saw it fail)
- Mocking domain objects (ShoppingItem mocked instead of real)
- Generic naming like "CanSetName" or "TestAddItem"
- More than 3 unrelated assertions without [Theory]
- Testing language features (property setters work)
- Using Moq instead of NSubstitute
- Test with zero assertions
- Rationalizing "write test after implementation this time"
- Integration test only checks `response.EnsureSuccessStatusCode()` without verifying response body

---

### react-tdd (Opus → Sonnet)

**What it does:** Enforces test-first development for React (Vitest + React Testing Library) — write failing test before implementation.

**Test input:**
"Implement a CartPage component that shows 'Your cart is empty' when there are no items, and displays item names when items exist."

**GREEN criteria (pass — Sonnet is sufficient):**
- Test written FIRST before any implementation code
- Test named using convention: `it('should show empty message when cart has no items')`
- Test uses real Zustand stores (reset in beforeEach), no vi.mock on stores
- MSW (Mock Service Worker) used for API mocking, not vi.mock on components/hooks
- Max 3 assertions per test or uses it.each for parameterized cases
- Test verified to fail correctly (missing component) before implementation
- Minimal implementation to pass (no extra features)
- Custom render wrapper with providers used for integration
- Tests user-visible behavior (what user sees/does), not implementation details

**RED indicators (fail — needs bigger model):**
- Implementation code written before test
- Test passes immediately (never saw it fail)
- `vi.mock` used on Zustand stores or React components
- Testing implementation details (className exists, prop structure)
- More than 3 unrelated assertions without it.each
- Test with zero assertions
- Testing that a mock was called instead of user-visible behavior
- Rationalizing "explore first, test later"
- Not using custom render wrapper for provider setup
- Missing MSW handlers for API boundaries, using fetch mocks instead

---

### deploy-to-marketplace (Opus → Sonnet)

**What it does:** Deploys skills from `~/.claude/skills/` to saurun-marketplace plugin repo with safety checks and conflict detection.

**Test input:**
"Deploy the 'web-design-wizard' skill to the marketplace."

**GREEN criteria (pass — Sonnet is sufficient):**
- Validates SKILL.md exists before proceeding
- Validates frontmatter (name, description) present
- Checks git state for uncommitted changes, warns user
- Compares modification times between local and marketplace versions
- Shows diff and asks user confirmation if marketplace version is newer
- Uses correct copy command for platform (rsync on Unix, cp on Windows)
- Calls `/publish-plugin bump` command (doesn't manually bump version)
- Copies entire skill directory including references/, scripts/, etc.
- All safety checks performed — never skipped

**RED indicators (fail — needs bigger model):**
- Skips validation checks ("time is tight")
- Doesn't detect or warn about marketplace being newer
- Manual version bumping instead of calling `/publish-plugin bump`
- Overwrites marketplace without showing diff or asking confirmation
- Only copies SKILL.md, ignores references/ and supporting files
- Doesn't check git state or uncommitted changes
- Wrong copy command for platform (uses Windows syntax on Unix)
- Proceeds with deployment despite validation failures
- Skips safety checks with rationalization ("skill is simple, checks unnecessary")
