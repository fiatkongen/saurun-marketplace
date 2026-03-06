---
name: dotnet-tactical-ddd
description: Use when building .NET backend APIs with business logic, domain rules, invariants to enforce, or projects expanding beyond simple CRUD. Symptoms - anemic models with public setters, domain logic in controllers, AutoMapper bypassing validation, primitives instead of value objects
---

# .NET Tactical DDD — Enforcement Protocol

Mandatory constraints for all .NET backend code. Violations must be fixed before committing. These rules override implementation plan structure when they conflict.

## Step 0: Install Base Classes (FIRST)

Before writing ANY entity, copy [base-classes.cs](base-classes.cs) into `Domain/Common/`. Every entity MUST inherit from these. If the project already has base classes, use those.

## Mandatory Rules

### 1. Base Class Inheritance

```csharp
public class Order : AggregateRoot<Guid> { ... }   // aggregate root
public class OrderLine : Entity<int> { ... }        // child entity
```

Bare `public class Order { ... }` is a violation.

### 2. Private Constructors + Result<T> Factories

Private constructor only. Public creation via static `Create()` returning `Result<T>`. Object initializers in factories are banned — use the constructor.

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

### 3. Zero Public/Init Setters in Domain

All properties: `{ get; private set; }`. `{ get; set; }` and `{ get; init; }` are violations. State changes only through behavior methods.

### 4. Domain Events on Every State Change

Every method that mutates state raises a domain event via `AddDomainEvent()`. Includes `Create()`. Events are immutable records, past tense.

```csharp
public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, DateTime OccurredAt) : IDomainEvent;
public record OrderShippedEvent(Guid OrderId, DateTime OccurredAt) : IDomainEvent;
```

A mutation without `AddDomainEvent()` is a violation.

### 5. All Mutations Return Result — Never Void

Every public state-changing method returns `Result` or `Result<T>`. Never `void`. Never throw exceptions for business rules. Application services also return `Result<T>`, not `Task<T?>`.

```csharp
public Result UpdateStatus(OrderStatus newStatus) {
    if (Status == OrderStatus.Completed) return Result.Failure("Cannot modify completed order");
    Status = newStatus;
    AddDomainEvent(new OrderStatusChangedEvent(Id, newStatus, DateTime.UtcNow));
    return Result.Success();
}
```

### 6. Value Objects — At Least One Per Aggregate

Every aggregate root MUST have at least one value object. An aggregate with zero VOs is a violation.

**Mandatory VO triggers** (convert to `ValueObject` if these exist as raw primitives):
- Money/currency → `Money`
- Email → `Email`
- URLs/repo paths → `RepoUrl`
- Names with constraints → `AgentName` (length, allowed chars)
- Typed IDs across aggregates → `BuildId`, `CustomerId`

When unsure if a primitive deserves a VO → make it a VO. Raw `decimal` for money is always a violation.

### 7. DTOs at API Boundaries

Controllers receive request DTOs, return response DTOs. Never expose entities. Map via extension methods: `ToDto()`, `ToDomain()`, `UpdateFromRequest()`.

### 8. Repository Per Aggregate

One interface per aggregate root. Controllers and services depend on the interface, never `DbContext`.

```csharp
public interface IOrderRepository {
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

`DbContext` in a controller or application service is a violation.

### 9. EF Core Configuration

Private setters FAIL at runtime without this. One `IEntityTypeConfiguration<T>` per entity.

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId");
        builder.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsOne(o => o.TotalPrice, p => {
            p.Property(m => m.Amount).HasColumnName("TotalAmount");
            p.Property(m => m.Currency).HasColumnName("TotalCurrency");
        });
        builder.Property(o => o.Status).HasConversion<string>();
    }
}
```

- `UsePropertyAccessMode(PropertyAccessMode.Field)` for every private collection
- `OwnsOne()` for every value object property
- Enums: `.HasConversion<string>()`
- Apply via `modelBuilder.ApplyConfigurationsFromAssembly(...)`

### 10. Layer Rule

Domain/ has zero Infrastructure dependencies. Controllers depend on Application services only.

`using Microsoft.EntityFrameworkCore` in Domain/ is a violation. `using Infrastructure` in Controllers/ is a violation.

## Pre-Commit Verification

**Run before committing. If ANY fails, fix it.**

```bash
# 1. Entities inherit base classes
grep -rL "AggregateRoot\|Entity<" Domain/Entities/ && echo "FAIL: bare entities"

# 2. No public/init setters
grep -rn "{ get; set; }\|{ get; init; }" Domain/Entities/ && echo "FAIL: public setters"

# 3. Domain events exist
grep -rn "IDomainEvent" Domain/ | grep -q "record" || echo "FAIL: no domain events"

# 4. No DbContext outside Infrastructure
grep -rn "DbContext" **/Controllers/ **/Services/ 2>/dev/null && echo "FAIL: DbContext leak"

# 5. Result<T> in factories
grep -rn "Result<" Domain/Entities/ | grep -q "." || echo "FAIL: no Result<T>"

# 6. No void mutations
grep -rn "public void" Domain/Entities/ && echo "FAIL: void mutations"

# 7. No object initializers in factories
grep -A5 "static.*Create" Domain/Entities/ | grep -q "new.*{" && echo "FAIL: object initializer"

# 8. Value objects exist
ls Domain/ValueObjects/*.cs 2>/dev/null | grep -q "." || echo "FAIL: no value objects"

# 9. Base classes installed
ls Domain/Common/*.cs 2>/dev/null | grep -q "." || echo "FAIL: no base classes"

# 10. EF Core configs exist
ls Infrastructure/Persistence/Configurations/*Configuration.cs 2>/dev/null | grep -q "." || echo "FAIL: no EF configs"

# 11. Layer isolation
grep -rn "using.*Infrastructure" **/Controllers/ 2>/dev/null && echo "FAIL: layer violation"
```
