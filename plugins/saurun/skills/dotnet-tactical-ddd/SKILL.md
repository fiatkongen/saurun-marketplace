---
name: dotnet-tactical-ddd
description: Use when building .NET backend APIs with business logic, domain rules, invariants to enforce, or projects expanding beyond simple CRUD. Symptoms - anemic models with public setters, domain logic in controllers, AutoMapper bypassing validation, primitives instead of value objects
---

# .NET Tactical DDD — Enforcement Protocol

This skill defines **mandatory constraints** on all .NET backend code you write. These are not suggestions. Code that violates these rules is broken and must be fixed before committing. These rules apply regardless of what the implementation plan specifies.

## Step 0: Install Base Classes (FIRST)

Before writing ANY entity code, copy [base-classes.cs](base-classes.cs) into `Domain/Common/`. This file provides `Result<T>`, `ValueObject`, `AggregateRoot<TId>`, `Entity<TId>`, and `IDomainEvent`.

**Every entity you write MUST inherit from these.** If the target project already has base classes, use those instead.

## Mandatory Rules

### 1. Every Entity Inherits Base Classes

```csharp
// REQUIRED — no exceptions
public class Order : AggregateRoot<Guid> { ... }
public class OrderLine : Entity<int> { ... }
```

Bare `public class Order { ... }` is a violation.

### 2. Private Constructors + Factory Returning Result<T>

Every entity has a private constructor. The only public creation path is a static `Create()` method returning `Result<T>`. If creation genuinely cannot fail, return the type directly — but this is rare.

```csharp
private Order() { } // EF Core
private Order(Guid id, Guid customerId) : base(id) {
    CustomerId = customerId;
    Status = OrderStatus.Pending;
}

public static Result<Order> Create(Guid customerId) {
    if (customerId == Guid.Empty) return Result.Failure<Order>("Customer ID required");
    var order = new Order(Guid.NewGuid(), customerId);
    order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, DateTime.UtcNow));
    return Result.Success(order);
}
```

`new Order()` from outside the class is a violation. `public Order(...)` is a violation.

**Object initializers in factories are banned.** `return new Order { Title = title }` bypasses the constructor chain and invariant protection. Always use the private constructor: `return new Order(id, title)`.

### 3. Zero Public Setters in Domain

All properties: `{ get; private set; }`. State changes happen through behavior methods that enforce invariants and raise events.

```csharp
public OrderStatus Status { get; private set; }

public Result Ship() {
    if (Status != OrderStatus.Ready) return Result.Failure("Order not ready");
    Status = OrderStatus.Shipped;
    AddDomainEvent(new OrderShippedEvent(Id, DateTime.UtcNow));
    return Result.Success();
}
```

`{ get; set; }` on a domain entity is a violation. `{ get; init; }` is also a violation — init setters allow external assignment at construction. Direct property assignment from outside the entity is a violation.

### 4. Domain Events on Every State Change

Every method that mutates state MUST raise a domain event via `AddDomainEvent()`. Events are immutable records, past tense. This includes `Create()` — entity creation is a state change.

```csharp
public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, DateTime OccurredAt) : IDomainEvent;
public record OrderShippedEvent(Guid OrderId, DateTime OccurredAt) : IDomainEvent;
public record OrderLineAddedEvent(Guid OrderId, Guid ProductId, int Quantity, DateTime OccurredAt) : IDomainEvent;
```

A state-changing method without `AddDomainEvent()` is a violation. A `Create()` method without an event is a violation.

### 5. All Mutation Methods Return Result — Never Void

Every public method that changes state MUST return `Result` or `Result<T>`. Never `void`. Never throw exceptions for business rule violations.

```csharp
// GOOD — returns Result, caller can handle failure
public Result UpdateStatus(OrderStatus newStatus) {
    if (Status == OrderStatus.Completed) return Result.Failure("Cannot modify completed order");
    Status = newStatus;
    AddDomainEvent(new OrderStatusChangedEvent(Id, newStatus, DateTime.UtcNow));
    return Result.Success();
}

// BAD — void, no error path, no event
public void UpdateStatus(OrderStatus newStatus) {
    Status = newStatus;
}
```

`public void` on a state-changing domain method is a violation. `throw new Exception("business rule")` is a violation.

**Application services also use Result<T>**, not nullable returns:
```csharp
// GOOD
public async Task<Result<BuildDto>> UpdateBuildAsync(int id, UpdateRequest request) { ... }

// BAD — nullable hides the failure reason
public async Task<BuildDto?> UpdateBuildAsync(int id, UpdateRequest request) { ... }
```

### 6. Value Objects for Domain Concepts

Strings/decimals/ints that represent domain concepts MUST be value objects inheriting `ValueObject`. Every aggregate root: identify at least two candidates. Common examples: Money, Email, Url, Quantity, typed IDs (CustomerId, OrderId).

**Rule of thumb:** If two different `string` properties mean different things (Name vs Email), at least one should be a value object. Raw `decimal` for money is always a violation.

### 7. DTOs at API Boundaries — Always

Controllers receive request DTOs, return response DTOs. Never expose domain entities. Map via extension methods: `ToDto()`, `ToDomain()`, `UpdateFromRequest()`.

### 8. Repository Per Aggregate

One interface per aggregate root in `Domain/Interfaces/` or `Application/Interfaces/`. Controllers and services depend on the interface, never on `DbContext` directly.

```csharp
public interface IOrderRepository {
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

`DbContext` injected into a controller or application service is a violation. Controllers must depend on application services or repository interfaces — never on infrastructure directly.

### 9. EF Core Configuration (Required for Private Setters to Work)

Private setters and private collections WILL FAIL at runtime without explicit EF Core configuration. This is not optional.

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        // Private collection — MUST configure backing field access
        builder.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId");
        builder.Navigation(o => o.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Value object as owned type
        builder.OwnsOne(o => o.TotalPrice, p => {
            p.Property(m => m.Amount).HasColumnName("TotalAmount");
            p.Property(m => m.Currency).HasColumnName("TotalCurrency");
        });

        // Enum stored as string
        builder.Property(o => o.Status).HasConversion<string>();
    }
}
```

**Rules:**
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- `UsePropertyAccessMode(PropertyAccessMode.Field)` for every private collection
- `OwnsOne()` for every value object property
- Enums: `.HasConversion<string>()`
- Apply via `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`
- All entities need parameterless `protected` constructor for EF (already in base-classes.cs)

Missing an EF config for a private collection = runtime crash. This is a violation.

### 10. Layer Rule

`Domain/` has zero dependencies on Infrastructure, EF Core, or any external package. `Api/Controllers/` must not import Infrastructure — they depend on Application services only.

```
Api/Controllers/ → Application/Services/ → Domain/ (via repository interfaces)
                                         → Infrastructure/ (implementations registered via DI)
```

`using Microsoft.EntityFrameworkCore` in Domain/ is a violation. `using Infrastructure.Persistence` in Api/Controllers/ is a violation.

## Pre-Commit Verification

**Run these checks before committing. If ANY fails, fix it.**

```bash
# 1. All entities must inherit base classes
grep -rL "AggregateRoot\|Entity<" Domain/Entities/ && echo "FAIL: entities not inheriting base classes"

# 2. No public setters in Domain entities
grep -rn "{ get; set; }\|{ get; init; }" Domain/Entities/ && echo "FAIL: public/init setters in domain"

# 3. Domain events exist (at least one record implementing IDomainEvent)
grep -rn "IDomainEvent" Domain/ | grep -q "record" || echo "FAIL: no domain event records found"

# 4. No DbContext in controllers or application services
grep -rn "DbContext" **/Controllers/ **/Services/ 2>/dev/null && echo "FAIL: DbContext leaked outside Infrastructure"

# 5. Result<T> used in entity factory methods
grep -rn "Result<" Domain/Entities/ | grep -q "." || echo "FAIL: no Result<T> return types in entities"

# 6. No void mutation methods in domain entities
grep -rn "public void" Domain/Entities/ && echo "FAIL: void mutation methods — must return Result"

# 7. No object initializers in factory methods
grep -A5 "static.*Create" Domain/Entities/ | grep -q "new.*{" && echo "FAIL: object initializer in factory — use constructor"

# 8. Base classes installed
ls Domain/Common/*.cs 2>/dev/null | grep -q "." || echo "FAIL: base classes missing in Domain/Common/"

# 9. EF Core configurations exist
ls Infrastructure/Persistence/Configurations/*Configuration.cs 2>/dev/null | grep -q "." || echo "FAIL: no EF Core entity configurations"

# 10. Layer isolation — no Infrastructure imports in Controllers
grep -rn "using.*Infrastructure" **/Controllers/ 2>/dev/null && echo "FAIL: Controllers importing Infrastructure"
```

All checks must pass. No exceptions. No "I'll fix it later."
