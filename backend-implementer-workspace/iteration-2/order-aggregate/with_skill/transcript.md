# Order Aggregate Implementation Transcript

## Skills Used

- `dotnet-tactical-ddd`: Rich domain models with behavior, Result<T>, ValueObject, AggregateRoot base classes, private constructors + static factory methods, domain events
- `dotnet-tdd`: Red-Green-Refactor cycle, test-first discipline, real domain objects (never mocked), max 3 assertions per test, Theory for parameterized cases

## Project Setup

Created .NET 9 solution with two projects:
- `Domain/` (Domain.csproj) - Pure business logic, no external dependencies
- `Tests/` (Tests.csproj) - xUnit test project referencing Domain

Copied base classes from skill reference (Result, ValueObject, Entity, AggregateRoot, IDomainEvent) into `Domain/Base/`.

Verified base classes compile before writing any tests.

---

## TDD Cycle 1: Money Value Object

### RED

Wrote 13 tests in `Tests/Unit/MoneyTests.cs`:
- `Create_WithValidAmountAndCurrency_ReturnsSuccessResult`
- `Create_WithZeroAmount_ReturnsSuccessResult`
- `Create_WithNegativeAmount_ReturnsFailure`
- `Create_WithInvalidCurrency_ReturnsFailure` (Theory: null, empty, whitespace)
- `Create_NormalizesCurrencyToUpperCase`
- `Equality_SameAmountAndCurrency_AreEqual`
- `Equality_DifferentAmount_AreNotEqual`
- `Equality_DifferentCurrency_AreNotEqual`
- `Add_SameCurrency_ReturnsSummedMoney`
- `Add_DifferentCurrency_ReturnsFailure`
- `Multiply_ByPositiveQuantity_ReturnsCorrectAmount`

**Build result:** FAIL - `The type or namespace name 'ValueObjects' does not exist in the namespace 'Domain'`

### GREEN

Implemented `Domain/ValueObjects/Money.cs`:
- Extends `ValueObject` base class
- Private constructor, `Create()` factory returning `Result<Money>`
- Validates: no negative amounts, currency required
- Normalizes currency to uppercase
- `Add()` returns `Result<Money>` (fails on currency mismatch)
- `Multiply(int)` returns new Money
- `Zero()` static helper for zero-amount initialization

**Test result:** 13/13 passed

### REFACTOR

No refactoring needed. Code is clean and minimal.

---

## TDD Cycle 2: Quantity Value Object

### RED

Wrote 7 tests in `Tests/Unit/QuantityTests.cs`:
- `Create_WithPositiveValue_ReturnsSuccess`
- `Create_WithOne_ReturnsSuccess`
- `Create_WithZeroOrNegative_ReturnsFailure` (Theory: 0, -1, -100)
- `Equality_SameValue_AreEqual`
- `Equality_DifferentValue_AreNotEqual`

**Build result:** FAIL - `The name 'Quantity' does not exist in the current context`

### GREEN

Implemented `Domain/ValueObjects/Quantity.cs`:
- Extends `ValueObject` base class
- Private constructor, `Create()` factory returning `Result<Quantity>`
- Validates: must be positive (>= 1)

**Test result:** 20/20 passed (13 Money + 7 Quantity)

### REFACTOR

No refactoring needed.

---

## TDD Cycle 3: OrderLineItem Entity + Order Aggregate Root

### RED

Wrote 3 tests in `Tests/Unit/OrderLineItemTests.cs`:
- `Create_WithValidInputs_ReturnsSuccess`
- `Create_WithEmptyProductId_ReturnsFailure`
- `LineTotal_ReturnsQuantityTimesUnitPrice`

Wrote 15 tests in `Tests/Unit/OrderTests.cs`:
- **Creation:** `Create_WithValidCustomerIdAndLineItem_ReturnsSuccess`, `Create_WithEmptyCustomerId_ReturnsFailure`, `Create_SetsStatusToPending`, `Create_RaisesOrderCreatedDomainEvent`
- **Adding items:** `AddLineItem_WithValidInputs_AddsToLineItems`, `AddLineItem_WithEmptyProductId_ReturnsFailure`
- **Max value ($10,000):** `AddLineItem_ExceedingMaxOrderValue_ReturnsFailure`, `Create_WithLineItemExceedingMaxOrderValue_ReturnsFailure`, `AddLineItem_ExactlyAtMaxOrderValue_Succeeds`
- **Confirmation:** `Confirm_PendingOrder_SetsStatusToConfirmed`, `Confirm_RaisesOrderConfirmedDomainEvent`, `AddLineItem_WhenConfirmed_ReturnsFailure`, `Confirm_AlreadyConfirmed_ReturnsFailure`
- **Total calculation:** `TotalAmount_WithMultipleLineItems_ReturnsSumOfLineTotals`, `TotalAmount_WithSingleLineItem_ReturnsLineTotal`

**Build result:** FAIL - `The type or namespace name 'Entities' does not exist in the namespace 'Domain'`

### GREEN

Implemented:

1. `Domain/Entities/OrderStatus.cs` - Enum: Pending, Confirmed
2. `Domain/Entities/OrderLineItem.cs` - Entity with ProductId, Quantity (value object), UnitPrice (value object), computed LineTotal
3. `Domain/Events/OrderCreatedEvent.cs` - Immutable record, past tense
4. `Domain/Events/OrderConfirmedEvent.cs` - Immutable record, past tense
5. `Domain/Entities/Order.cs` - Aggregate root with:
   - Private `_lineItems` collection, exposed as `IReadOnlyList`
   - `Create()` factory requiring at least one line item (enforces "must have at least one" invariant)
   - `AddLineItem()` with confirmed-order guard and $10,000 max value check
   - `Confirm()` with idempotency guard
   - `TotalAmount` computed property summing line totals
   - Domain events raised on creation and confirmation

**Test result:** 38/38 passed

### REFACTOR

Fixed nullable warnings on EF Core parameterless constructor by adding `= null!` to reference-type properties.

**Test result after refactor:** 38/38 passed, 0 warnings

---

## Final Results

```
Passed!  - Failed: 0, Passed: 38, Skipped: 0, Total: 38
Build succeeded. 0 Warning(s), 0 Error(s)
```

## Business Rules Covered

| Rule | Enforcement | Test(s) |
|------|------------|---------|
| Order must have at least one line item | `Create()` requires first line item | `Create_WithValidCustomerIdAndLineItem_ReturnsSuccess` |
| Quantity must be positive (>= 1) | `Quantity.Create()` rejects 0 and negative | Theory: 0, -1, -100 |
| UnitPrice must be non-negative | `Money.Create()` rejects negative amounts | `Create_WithNegativeAmount_ReturnsFailure` |
| Total cannot exceed $10,000 | Checked in both `Create()` and `AddLineItem()` | 3 tests (exceed, exact boundary, creation) |
| Confirmed orders reject new items | `AddLineItem()` checks `Status == Confirmed` | `AddLineItem_WhenConfirmed_ReturnsFailure` |

## DDD Patterns Applied

- **Value Objects:** Money (amount + currency), Quantity (positive int) -- immutable, value equality via `ValueObject` base
- **Entity:** OrderLineItem -- identity via Guid, private setters
- **Aggregate Root:** Order -- single entry point, enforces all invariants, private collection
- **Factory Methods:** All domain objects use static `Create()` returning `Result<T>`, private constructors
- **Domain Events:** OrderCreatedEvent, OrderConfirmedEvent -- immutable records, past tense
- **Result<T>:** All fallible operations return Result, no exceptions for business rules
- **No Anemic Models:** Order has behavior (AddLineItem, Confirm, TotalAmount), not just data

## File Listing

```
outputs/
  OrderAggregate.sln
  Domain/
    Domain.csproj
    Base/
      Result.cs
      ValueObject.cs
      Entity.cs
      AggregateRoot.cs
      IDomainEvent.cs
    ValueObjects/
      Money.cs
      Quantity.cs
    Entities/
      Order.cs
      OrderLineItem.cs
      OrderStatus.cs
    Events/
      OrderCreatedEvent.cs
      OrderConfirmedEvent.cs
  Tests/
    Tests.csproj
    GlobalUsings.cs
    Unit/
      MoneyTests.cs
      QuantityTests.cs
      OrderLineItemTests.cs
      OrderTests.cs
```
