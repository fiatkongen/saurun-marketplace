# Transcript: Order Aggregate Root (Without Skill)

## Task
Implement a .NET 9 Order aggregate root with DDD patterns including:
- Value objects (Money, Quantity)
- Result<T> pattern (no exceptions)
- Business rule enforcement via invariants
- xUnit test coverage for all rules and edge cases

## Approach

### 1. Domain Model Design

Started by identifying the key domain concepts:

- **Value Objects:** `Money` (amount + currency) and `Quantity` (positive integer wrapper)
- **Entity:** `OrderLineItem` with ProductId, Quantity, and UnitPrice
- **Aggregate Root:** `Order` managing a collection of line items with status transitions
- **Supporting Types:** `Result<T>` for railway-oriented error handling, `Currency` enum, `OrderStatus` enum

### 2. Implementation Order

Created files in dependency order:
1. `Result<T>` - generic result type with Success/Failure factory methods and Match for pattern matching
2. `Currency` enum - USD, EUR, GBP, DKK
3. `Money` value object - immutable record with Create (validates non-negative), Add (validates same currency), Multiply
4. `Quantity` value object - immutable record with Create (validates >= 1)
5. `OrderStatus` enum - Draft, Confirmed
6. `OrderLineItem` entity - private constructor, factory Create method, LineTotal computation
7. `Order` aggregate root - enforces all business invariants

### 3. Key Design Decisions

- **Money as `sealed record`**: Gets value equality for free (two Money with same amount+currency are equal). Immutable by design.
- **Quantity as `sealed record`**: Same rationale -- value semantics, immutability.
- **Order.Create requires initial line item**: The requirement "must have at least one line item" is enforced at creation time by requiring the first item in the factory method. This makes it impossible to construct an empty order.
- **Private constructors everywhere**: All domain objects use private constructors + static factory methods returning `Result<T>`. No way to bypass validation.
- **Max value check on AddLineItem**: Calculates prospective total before adding. If it would exceed $10,000, the item is rejected and the order remains unchanged.
- **Confirmed order immutability**: `AddLineItem` checks status first and rejects additions to confirmed orders.
- **Currency consistency**: Order has a fixed currency; all line item prices must match.

### 4. Invariants Enforced

| Rule | Where Enforced |
|------|---------------|
| Order must have >= 1 line item | `Order.Create` requires first item |
| Quantity >= 1 | `Quantity.Create` |
| UnitPrice >= 0 | `Money.Create` |
| Total <= $10,000 | `Order.AddLineItemInternal` checks prospective total |
| No items after confirmation | `Order.AddLineItem` checks status |
| Currency consistency | `Order.Create` and `Order.AddLineItem` |
| ProductId not empty | `OrderLineItem.Create` |

### 5. Test Coverage

**44 tests total**, all passing:

- **MoneyTests (13):** Create valid/zero/negative, rounding, Add same/different currency, Multiply positive/zero/negative, Zero factory, equality (same, different amount, different currency), ToString
- **QuantityTests (7):** Create valid/one/zero/negative, equality same/different, ToString
- **OrderLineItemTests (4):** Create valid/empty ProductId, LineTotal with multiple quantity, LineTotal single quantity
- **OrderTests (20):** Create valid/currency mismatch/exceeds max/exactly at max, AddLineItem to draft/confirmed/currency mismatch/exceeds max/at max/empty ProductId/accumulates total, Confirm draft/already confirmed, CalculateTotal single/multiple items, encapsulation (read-only list), edge cases (zero price, many small items, non-USD currency)

### 6. Build Issues

Initial build failed because test files were missing `using Xunit;` -- the `ImplicitUsings` in .NET only covers `System` namespaces, not third-party packages. Fixed by adding the import to all four test files.

## Final Result

- **Build:** 0 warnings, 0 errors
- **Tests:** 44 passed, 0 failed
- **Time:** ~1 second total build + test

## Files Created

```
outputs/
  OrderAggregate.sln
  Domain/
    Domain.csproj
    Result.cs
    Currency.cs
    Money.cs
    Quantity.cs
    OrderStatus.cs
    OrderLineItem.cs
    Order.cs
  Tests/
    Tests.csproj
    MoneyTests.cs
    QuantityTests.cs
    OrderLineItemTests.cs
    OrderTests.cs
```
