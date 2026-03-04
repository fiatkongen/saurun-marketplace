# Transcript: Order Aggregate Root (Without Skill)

## Task

Implement a .NET 9 Order aggregate root with value objects (Money, Quantity), Result<T> pattern, business rule enforcement, and comprehensive xUnit tests.

## Approach

Worked bottom-up: foundational types first, then entities, then aggregate root, then tests.

## Steps

### 1. Created project structure
- `Domain/Domain.csproj` targeting net9.0
- `Tests/Tests.csproj` targeting net9.0 with xunit 2.9.3, xunit.runner.visualstudio 2.8.2, Microsoft.NET.Test.Sdk 17.12.0
- `OrderAggregate.sln` linking both projects

### 2. Implemented domain types (Domain/)

**Result.cs** — Generic Result<T> monad with Map/Bind for railway-oriented error handling. No exceptions for domain validation failures.

**Currency.cs** — Simple enum (USD, EUR, GBP, DKK).

**Money.cs** — Value object (sealed record). Private constructor, factory `Create()` validates non-negative amount and rounds to 2 decimal places. Supports `Add` (same-currency only) and `Multiply` (by non-negative int). `Zero()` factory for accumulator seed.

**Quantity.cs** — Value object (sealed record). Enforces minimum value of 1.

**OrderStatus.cs** — Enum with Draft and Confirmed states.

**OrderLineItem.cs** — Entity within the aggregate. Factory validates non-empty ProductId. `LineTotal()` computes UnitPrice * Quantity.

**Order.cs** — Aggregate root. Key design decisions:
- Constructor is private; `Create()` factory requires an initial line item (enforces "at least one item" invariant at creation time, not just at confirmation)
- `AddLineItem()` checks: (a) order is Draft, (b) value objects are valid, (c) prospective total <= $10,000
- `Confirm()` transitions to Confirmed; idempotent double-confirm returns failure
- `_lineItems` is private `List<OrderLineItem>`, exposed as `IReadOnlyList<OrderLineItem>` — prevents external mutation
- All mutating operations return `Result<Order>` so callers can chain or inspect errors

### 3. Implemented tests (Tests/)

**ResultTests.cs** (11 tests) — Success/failure construction, Value access on failure throws, Map/Bind chaining, error propagation.

**MoneyTests.cs** (13 tests) — Valid/zero/negative creation, rounding, Zero factory, Add same/different currency, Multiply positive/zero/negative, equality by value+currency, ToString.

**QuantityTests.cs** (7 tests) — Positive/one/zero/negative creation, equality, ToString.

**OrderLineItemTests.cs** (4 tests) — Valid creation, empty ProductId, LineTotal calculation, single-quantity identity.

**OrderTests.cs** (27 tests) covering:
- Creation: valid, empty OrderId, zero/negative quantity, negative price, empty ProductId, zero price, exceeding max, at exact max, quantity*price exceeding max
- AddLineItem: to draft, zero quantity, negative price, exceeding max (item not added), reaching exact max, multiple items accumulation, empty ProductId
- Confirm: draft order, already confirmed, add after confirm (rejected + item count unchanged)
- CalculateTotal: single item, multiple items
- Encapsulation: readonly list, starts as Draft
- Edge cases: 1 cent over max, exact max after multiple adds, large quantity * small price, zero-price item

### 4. Build & test

**First build failed:** Missing `using Xunit;` in test files (ImplicitUsings doesn't include xUnit namespaces). Added the using directive to all 5 test files.

**Second build:** 0 warnings, 0 errors.

**Test run:** 62 tests passed, 0 failed. Total time: 0.26 seconds.

## File Inventory

| File | Purpose |
|------|---------|
| `Domain/Result.cs` | Result<T> monad |
| `Domain/Currency.cs` | Currency enum |
| `Domain/Money.cs` | Money value object |
| `Domain/Quantity.cs` | Quantity value object |
| `Domain/OrderStatus.cs` | Order lifecycle enum |
| `Domain/OrderLineItem.cs` | Line item entity |
| `Domain/Order.cs` | Aggregate root |
| `Domain/Domain.csproj` | Domain project file |
| `Tests/ResultTests.cs` | 11 tests |
| `Tests/MoneyTests.cs` | 13 tests |
| `Tests/QuantityTests.cs` | 7 tests |
| `Tests/OrderLineItemTests.cs` | 4 tests |
| `Tests/OrderTests.cs` | 27 tests |
| `Tests/Tests.csproj` | Test project file |
| `OrderAggregate.sln` | Solution file |

**Total: 62 tests, all passing.**

## Observations

- Required one fix cycle: forgot `using Xunit;` directive in test files since .NET implicit usings don't cover third-party namespaces
- The "order must have at least one line item" invariant is enforced at creation time by requiring an initial item in the `Create` factory, rather than allowing empty orders and checking on confirm
- Money arithmetic returns Result<Money> to stay consistent with the no-exceptions pattern throughout
