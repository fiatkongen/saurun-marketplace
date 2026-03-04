---
name: dotnet-tactical-ddd
description: Use when building .NET backend APIs with business logic, domain rules, invariants to enforce, or projects expanding beyond simple CRUD. Symptoms - anemic models with public setters, domain logic in controllers, AutoMapper bypassing validation, primitives instead of value objects
---

# .NET Tactical Domain-Driven Design

Rich domain models with behavior, not anemic data containers. Domain models encapsulate business rules and invariants. DTOs at API boundaries. Manual mapping via extension methods. Never expose domain models in APIs.

**Base classes:** See [base-classes.cs](base-classes.cs) for `Result<T>`, `ValueObject`, `AggregateRoot<T>`, `IDomainEvent`. Copy into `Domain/`.

## Critical Patterns (These Don't Happen Without Guidance)

### 1. Domain Events — Always Raise Them

Every significant state change raises an immutable record event (past tense). This is the most commonly missed pattern. Without domain events, aggregates become tightly coupled and side effects get scattered across services.

```csharp
public record OrderShippedEvent(Guid OrderId, Guid CustomerId, DateTime OccurredAt) : IDomainEvent;

public class Order : AggregateRoot<Guid> {
    public static Result<Order> Create(Guid customerId) {
        var order = new Order(Guid.NewGuid(), customerId);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, DateTime.UtcNow));
        return Result.Success(order);
    }

    public Result Ship() {
        if (Status != OrderStatus.Ready) return Result.Failure("Order not ready");
        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedEvent(Id, CustomerId, DateTime.UtcNow));
        return Result.Success();
    }
}
```

**Rules:** One event per state change. Immutable records. Past tense names. Raised inside the aggregate, not from services. Use `AggregateRoot<T>.AddDomainEvent()` from base-classes.cs.

### 2. Private Constructors + Static Factories Returning Result<T>

Prevents invalid object creation. The factory is the only public way to construct the object. If a factory has no failure modes, it may return the type directly.

```csharp
public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public Money Price { get; private set; }

    private Product(Guid id, string name, Money price) : base(id) {
        Name = name; Price = price;
    }
    private Product() { } // EF Core

    public static Result<Product> Create(string name, Money price) {
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure<Product>("Name required");
        if (price.Amount <= 0) return Result.Failure<Product>("Price must be positive");
        return Result.Success(new Product(Guid.NewGuid(), name, price));
    }
}
```

### 3. Money as a Value Object (Never Raw Decimal)

Domain concepts involving money, quantities, or identifiers deserve value objects — not raw primitives. `Money` is the most commonly skipped one.

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency) { Amount = amount; Currency = currency; }

    public static Result<Money> Create(decimal amount, string currency) {
        if (amount < 0) return Result.Failure<Money>("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency)) return Result.Failure<Money>("Currency required");
        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    // Add natural operators where they make domain sense
    public Money Add(Money other) {
        if (Currency != other.Currency) throw new InvalidOperationException("Currency mismatch");
        return new Money(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents() {
        yield return Amount; yield return Currency;
    }
}
```

## Standard Patterns (Brief Reminders)

These patterns are well-known — brief reminders of the key rules:

**Entities & Aggregates:** Private setters + behavior methods. Reference other aggregates by ID only. Private backing collections with `IReadOnlyList` projection. Enforce all invariants inside the aggregate boundary.

**Result<T>:** Use for all operations that can fail. Non-generic `Result` for void operations. Never throw exceptions for business rule violations. See base-classes.cs for implementation.

**Repositories:** One interface per aggregate root. Eager load to avoid N+1. Specific interfaces, not generic `IRepository<T>`.

**DTOs:** Immutable records at API boundaries. Extension methods for mapping (`ToDto()`, `ToDomain()`, `UpdateFromRequest()`). Never AutoMapper — manual mapping gives control and respects factories.

**EF Core:** Private collections need `UsePropertyAccessMode(PropertyAccessMode.Field)`. Value objects use `OwnsOne()`. All entities need parameterless `protected` constructor.

**Layer rule:** Domain has zero external dependencies. Infrastructure → Application → Domain.

## Pragmatic Quality

DDD patterns exist to serve the domain, not the other way around. In addition to structural patterns:

- **Value objects should have natural operators** where they make domain sense (`+`, `-`, `>`, `<` on Quantity, Money)
- **Aggregates should be proactive** — e.g., an inventory aggregate should expire overdue reservations before checking availability, not wait for an external scheduler
- **Domain services should include batch operations** (e.g., `ProcessExpiredTrialsAsync`) and guard against duplicates where applicable
- **Time abstractions** (`IDateTimeProvider`) make state-transition logic testable — inject into services, pass `DateTime utcNow` to entity methods

## Red Flags

| Violation | Fix |
|-----------|-----|
| No domain events on state changes | Add immutable record events in past tense |
| Public constructor on entities | Private ctor + `Create()` returning `Result<T>` |
| `decimal` for money amounts | `Money` value object |
| `throw new Exception()` for business rules | Return `Result<T>` |
| Public setters on domain entities | Private setters + behavior methods |
| Domain models in controller params/returns | DTOs with manual mapping |
