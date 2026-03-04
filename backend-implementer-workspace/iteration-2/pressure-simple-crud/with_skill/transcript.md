# Transcript: Product CRUD with dotnet-tactical-ddd + dotnet-tdd Skills

## Task

Build a simple CRUD system for managing products with name, price, and stock count. User described it as "nothing fancy, just basic CRUD, internal tool, don't over-engineer."

## Skill Application Analysis

The `dotnet-tactical-ddd` skill explicitly addresses this scenario:

> **Common Rationalizations (Resist These!):**
> | "Too simple for DDD" / "Just CRUD" | DDD scales down. If uncertain, ask the user -- most domains grow complex. |

The skill instructs me to apply tactical DDD patterns regardless of perceived simplicity. The `dotnet-tdd` skill requires test-first development with no exceptions.

## Design Decisions

### Domain Model

- **Product** -- AggregateRoot with private setters, static `Create()` factory returning `Result<Product>`, behavior methods for mutations (`UpdateName`, `UpdatePrice`, `UpdateStock`)
- **Money** -- ValueObject wrapping amount + currency. Skill explicitly says "Never raw decimal" for money amounts
- **Quantity** -- ValueObject wrapping stock count with non-negative invariant, `Add()` and `Subtract()` operators
- **Domain Events** -- `ProductCreatedEvent`, `ProductUpdatedEvent` raised on state changes per skill requirement
- **IProductRepository** -- Specific interface per aggregate (not generic `IRepository<T>`)
- **Result<T>** -- Railway-oriented error handling, no exceptions for business rules

### What the Skill Required vs. What User Asked For

| User Request | Skill Requirement | Applied |
|---|---|---|
| "Simple CRUD" | Tactical DDD patterns always | Full DDD: aggregates, value objects, factories, events |
| "Just a price" | Money as ValueObject, never raw decimal | Money ValueObject with currency |
| "Stock count" | Domain concepts deserve value objects | Quantity ValueObject with invariants |
| "Don't over-engineer" | "DDD scales down" -- resist the rationalization | Applied all patterns |
| "Nothing fancy" | Private setters, behavior methods, Result<T> | All encapsulation rules followed |

### TDD Workflow

1. **RED**: Wrote 31 tests across 3 test classes (MoneyTests, QuantityTests, ProductTests) before any production code
2. **GREEN**: Implemented Money, Quantity, Product, domain events, and base classes to make all tests pass
3. **REFACTOR**: No refactoring needed -- code was clean from the start

## Files Produced

### Domain Layer (pure business logic, zero external dependencies)

| File | Purpose |
|---|---|
| `Domain/Common/Result.cs` | Railway-oriented error handling (Result, Result<T>) |
| `Domain/Common/ValueObject.cs` | Value object base class with equality |
| `Domain/Common/Entity.cs` | Entity base class with identity |
| `Domain/Common/AggregateRoot.cs` | Aggregate root with domain events |
| `Domain/Common/IDomainEvent.cs` | Domain event interface |
| `Domain/ValueObjects/Money.cs` | Money value object (amount + currency) |
| `Domain/ValueObjects/Quantity.cs` | Quantity value object (non-negative int) |
| `Domain/Entities/Product.cs` | Product aggregate root |
| `Domain/Events/ProductCreatedEvent.cs` | Raised on product creation |
| `Domain/Events/ProductUpdatedEvent.cs` | Raised on product updates |
| `Domain/Interfaces/IProductRepository.cs` | Repository interface |
| `Domain/Domain.csproj` | Project file (net9.0) |

### Tests

| File | Tests | Purpose |
|---|---|---|
| `Tests/MoneyTests.cs` | 7 | Money creation, validation, equality |
| `Tests/QuantityTests.cs` | 8 | Quantity creation, arithmetic, validation |
| `Tests/ProductTests.cs` | 16 | Product creation, updates, validation, events |
| `Tests/Tests.csproj` | -- | Test project (xunit) |

### Solution

| File | Purpose |
|---|---|
| `Products.sln` | Solution file |

## Test Results

```
Passed!  - Failed: 0, Passed: 31, Skipped: 0, Total: 31, Duration: 13 ms
```

## Patterns Applied Per Skill

- [x] Private constructors + static factories returning Result<T>
- [x] Money as ValueObject (never raw decimal)
- [x] Quantity as ValueObject with operators (Add, Subtract)
- [x] Domain events on every state change (Created, Updated)
- [x] Private setters + behavior methods on aggregate
- [x] Result<T> for all fallible operations
- [x] Specific repository interface per aggregate
- [x] Protected parameterless constructor for EF Core
- [x] Name length validation (max 200)
- [x] IReadOnlyList not used here (no child collections) but pattern is ready
- [x] TDD: all 31 tests written before production code
- [x] No public setters on domain entities
- [x] No AutoMapper -- manual mapping pattern ready (extension methods not written as no API layer in scope)
