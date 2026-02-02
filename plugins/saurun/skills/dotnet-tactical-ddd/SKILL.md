---
name: dotnet-tactical-ddd
description: Use when building .NET backend APIs with business logic, domain rules, invariants to enforce, or projects expanding beyond simple CRUD
---

# .NET Tactical Domain-Driven Design

## Overview

**Rich domain models with behavior, not anemic data containers.** Apply tactical DDD patterns to all .NET backend development for maintainable, testable, domain-centric code.

**Core principle:** Domain models encapsulate business rules and invariants. DTOs are immutable data contracts at API boundaries. Manual mapping via extension methods. Never expose domain models in APIs.

## 0. The Dependency Rule (Foundation)

**Dependencies point inward only.** Outer layers depend on inner layers, never the reverse.

```
Infrastructure → Application → Domain
   (adapters)     (use cases)    (core)
```

**Violations to catch:**
- Domain/ importing `Microsoft.EntityFrameworkCore`, `System.Net.Http`, `Serilog`
- Controllers calling repositories directly (bypassing Application/ use cases)
- Entities depending on application services
- Application/ directly instantiating Infrastructure/ classes (should use DI)

**Verification test:** "Can I run domain logic from unit tests with no database/HTTP/file system?" If yes, boundaries are correct.

## Layer Decision Tree

**Where does this code go?**

| Code Type | Layer | Example |
|-----------|-------|---------|
| Pure business logic, no I/O | `Domain/Entities/` or `Domain/ValueObjects/` | `OrderLine.CalculateTotal()` |
| Business rule across entities | `Domain/Services/` | `IPricingService.CalculateDiscount()` |
| Orchestrates domain + calls infra | `Application/{UseCase}/Handler` | `CreateOrderHandler` |
| Interface for external dependency | `Domain/Interfaces/` (if domain needs it)<br>`Application/Common/Interfaces/` (if only app uses) | `IEmailService` interface |
| Database implementation | `Infrastructure/Persistence/` | `OrderRepository` |
| HTTP/external API call | `Infrastructure/Services/` | `SmtpEmailService` |
| Request/Response DTO | `WebApi/DTOs/` | `CreateOrderRequest` |
| Extension methods for mapping | `WebApi/Extensions/` or `Application/Common/Mapping/` | `ProductMapper.ToDto()` |

## When to Use

**Apply tactical DDD when:**
- Building business logic beyond CRUD operations
- Domain rules and invariants need enforcement
- Business concepts need clear boundaries
- Project may expand beyond simple CRUD (most do)

**Skip tactical DDD ONLY when:**
- Absolutely certain code remains simple CRUD forever
- Overhead clearly outweighs complexity
- **When uncertain, ASK user first** - only they know if project will expand

## Quick Reference

| Pattern | Purpose | Example |
|---------|---------|---------|
| **Value Objects** | Immutable, value equality | `Money`, `EmailAddress`, `Quantity` |
| **Entities** | Identity, private setters | `Customer`, `OrderLine` |
| **Aggregates** | Transaction boundaries | `Order`, `Product`, `ShoppingCart` |
| **Factories** | Create valid objects | `Product.Create()`, `IOrderFactory` |
| **Repositories** | Persist/retrieve aggregates | `IOrderRepository` |
| **Domain Events** | Decouple aggregates | `OrderShippedEvent` |
| **Domain Services** | Multi-aggregate logic | `IPricingService` |
| **DTOs** | API boundaries | `CreateProductRequest`, `ProductDto` |
| **Result<T>** | Railway-oriented errors | `Result<Product>` |

## 1. Value Objects

Immutable objects identified by values, not identity. Use for measurements, money, quantities, descriptive characteristics.

```csharp
// ✅ Good: Immutable with factory validation
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency) {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string currency) {
        if (amount < 0) return Result.Failure<Money>("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency required");
        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    protected override IEnumerable<object> GetEqualityComponents() {
        yield return Amount;
        yield return Currency;
    }
}

// ❌ Bad: Mutable, no validation
public class Money {
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
```

## 2. Entities & Aggregates

**Entities:** Objects with unique identity. **Aggregates:** Transaction boundaries enforcing invariants.

**Aggregate rules:**
- Reference other aggregates by ID only (never full entity)
- Enforce invariants across all child entities
- Single entry point for modifications

```csharp
// ✅ Good: Aggregate with behavior and invariants
public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    private Order(Guid id, Guid customerId) : base(id) {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
    }

    public static Result<Order> Create(Guid customerId) {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>("Customer ID required");

        var order = new Order(Guid.NewGuid(), customerId);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return Result.Success(order);
    }

    public Result AddLine(Product product, int quantity) {
        if (Status == OrderStatus.Completed)
            return Result.Failure("Cannot modify completed order");
        if (quantity <= 0)
            return Result.Failure("Quantity must be positive");

        _lines.Add(OrderLine.Create(product.Id, product.Price, quantity));
        return Result.Success();
    }
}

// ❌ Bad: Anemic entity with public setters
public class Order {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderLine> Lines { get; set; }
}
```

## 3. Factories

Always use static factory methods returning `Result<T>`. Private constructors prevent invalid state.

```csharp
// ✅ Good: Factory with validation
public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public Money Price { get; private set; }

    private Product(Guid id, string name, Money price) : base(id) {
        Name = name;
        Price = price;
    }

    public static Result<Product> Create(string name, Money price) {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>("Name required");
        if (name.Length > 200)
            return Result.Failure<Product>("Name too long");
        if (price.Amount <= 0)
            return Result.Failure<Product>("Price must be positive");

        return Result.Success(new Product(Guid.NewGuid(), name, price));
    }
}

// ❌ Bad: Public constructor allows invalid state
public class Product {
    public string Name { get; set; }
    public Product() { }
}
```

## 4. Repositories

Abstraction per aggregate. Acts like a collection. Use eager loading to avoid N+1 queries.

```csharp
// ✅ Good: Specific interface per aggregate
public interface IOrderRepository {
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public class OrderRepository : IOrderRepository {
    private readonly ApplicationDbContext _context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) {
        return await _context.Orders
            .Include(o => o.Lines) // Eager loading
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct) {
        await _context.SaveChangesAsync(ct);
    }
}

// ❌ Bad: Generic repository
public interface IRepository<T> {
    Task<IEnumerable<T>> GetAllAsync();
}
```

## 5. Domain Events

Immutable records (past tense) decoupling aggregates. Use handlers for cross-aggregate logic.

```csharp
// ✅ Good: Immutable record, past tense
public record OrderShippedEvent(
    Guid OrderId,
    Guid CustomerId,
    DateTime OccurredAt
) : IDomainEvent;

public class Order : AggregateRoot<Guid> {
    public void Ship() {
        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedEvent(Id, CustomerId, DateTime.UtcNow));
    }
}

public class AwardLoyaltyPointsHandler : IDomainEventHandler<OrderShippedEvent> {
    public async Task Handle(OrderShippedEvent @event, CancellationToken ct) {
        var customer = await _customerRepo.GetByIdAsync(@event.CustomerId, ct);
        customer?.AwardPoints(100);
        await _customerRepo.SaveChangesAsync(ct);
    }
}

// ❌ Bad: Mutable, present tense
public class ShipOrder {
    public Guid OrderId { get; set; }
}
```

## 6. DTOs at API Boundaries

Never expose domain models in controllers. Use immutable records with extension methods for mapping.

**Naming:** `CreateXRequest`, `UpdateXRequest`, `XQuery` (requests) | `XDto` (responses)

```csharp
// DTOs
public record CreateProductRequest(string Name, decimal Price, string Currency);
public record ProductDto(Guid Id, string Name, decimal Price, string Currency);

// Extension methods for mapping
public static class ProductMapper
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id, product.Name, product.Price.Amount, product.Price.Currency);

    public static Result<Product> ToDomain(this CreateProductRequest request)
    {
        var priceResult = Money.Create(request.Price, request.Currency);
        if (priceResult.IsFailure) return Result.Failure<Product>(priceResult.Error);
        return Product.Create(request.Name, priceResult.Value);
    }
}

// Controller
[HttpPost]
public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
{
    var result = request.ToDomain();
    if (result.IsFailure) return BadRequest(result.Error);

    await _repository.AddAsync(result.Value);
    await _repository.SaveChangesAsync();
    return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value.ToDto());
}

// ❌ Bad: Domain in API
[HttpPost]
public async Task<ActionResult<Product>> Create(Product product) {
    return Ok(product);
}
```

**Security:** DTOs prevent over-posting attacks. Domain controls privileged fields.

```csharp
// ✅ Good: Safe DTO
public record CreateUserRequest(string Name);

// Domain controls IsAdmin, not client
var result = User.Create(request.Name);
```

**Pagination:**
```csharp
public record ProductQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 20);
public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);
```

## 7. Structured Logging

Use Serilog with structured properties (not string interpolation) for queryable logs.

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) => config
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

// ✅ Good: Structured properties
_logger.LogInformation(
    "Creating product {ProductName} with price {Price}",
    request.Name, request.Price);

// ❌ Bad: String interpolation
_logger.LogInformation($"Creating product {request.Name}");
```

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Public setters on entities | Private setters + behavior methods |
| Primitives for domain concepts | Value objects (`Money`, `EmailAddress`) |
| `new Product()` direct instantiation | `Product.Create()` factory method |
| Throwing exceptions for validation | Return `Result<T>` |
| Exposing domain models in API | Use DTOs with manual mapping |
| Inline DTO mapping | Extension methods (`ToDto()`, `ToDomain()`) |
| Generic `IRepository<T>` | Specific interfaces per aggregate |
| DbContext in controllers | Repository pattern |
| Bidirectional navigation | Reference by ID only |

## Red Flags - STOP and Fix

These indicate violation of tactical DDD:

- Public setters on domain entities
- `decimal` instead of `Money` value object
- `new Product { Name = ... }` object initializer
- Domain models in controller parameters/returns
- Inline DTO mapping in controllers
- `throw new Exception()` for business rule violations
- DbContext injected into controllers
- Full entity references across aggregates

**When you see these: Refactor immediately. These are architectural violations.**

## Common Rationalizations (Resist These!)

| Excuse | Reality |
|--------|---------|
| "It's just a simple bool flag" | Simple fields require encapsulation, behavior methods, domain events |
| "This is too simple for tactical DDD" | Tactical DDD scales down - simple domains just have fewer patterns |
| "Time pressure - skip the patterns" | Patterns prevent future bugs. Shortcuts create technical debt. |
| "Public setters are standard C#" | In DTOs yes, in domain models no. Encapsulation is non-negotiable. |
| "AutoMapper is industry standard" | Manual mapping gives control. AutoMapper bypasses factories and validation. |
| "Just CRUD, no business logic" | If truly just CRUD, user should confirm. Most "simple" domains grow complex. |
| "PM says 5 minutes, can't do full DDD" | Communicate reality. Bad architecture costs more time later. |

**All of these mean: Apply tactical DDD correctly. These rationalizations lead to anemic domain models.**

## Project Structure

```
Solution.sln
├── Domain/                     # Pure business logic
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Exceptions/
│   └── Interfaces/
├── Application/                # Use cases, orchestration
│   ├── Products/
│   │   ├── Commands/
│   │   └── Queries/
│   └── Common/
│       └── Interfaces/
├── Infrastructure/             # External concerns
│   ├── Persistence/
│   │   ├── Configurations/
│   │   └── Repositories/
│   └── Services/
└── WebApi/                     # API layer
    ├── Controllers/
    ├── DTOs/
    └── Extensions/
```

## Summary

**Key Principles:**
1. **Encapsulation** - Hide state, expose behavior
2. **Immutability** - Value objects are immutable
3. **Validation** - Always validate at creation (factories)
4. **Rich Models** - Behavior in entities, not just data
5. **Result<T>** - Railway-oriented error handling
6. **DTO Boundaries** - Never expose domain models
7. **Manual Mapping** - Extension methods, no AutoMapper
8. **Aggregate Boundaries** - Reference by ID only
9. **Specific Repositories** - Not generic, eager load, SaveChanges
10. **Domain Events** - Decouple aggregates

**Naming Conventions:**
- Requests: `CreateXRequest`, `UpdateXRequest`, `XQuery`
- Responses: `XDto`, `XDetailDto`, `XSummaryDto`
- Domain: `X` (entity), no suffixes

**Common Patterns:**
- `CreateProductRequest` → `ToDomain()` → `Product.Create()` → `Result<Product>`
- `Product` → `ToDto()` → `ProductDto`
- `UpdateProductRequest` → `UpdateFromRequest()` → `Result`
- Log with structured properties, not string interpolation
