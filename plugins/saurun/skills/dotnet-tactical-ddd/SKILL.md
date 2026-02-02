---
name: dotnet-tactical-ddd
description: Use when building .NET 8 backend APIs with business logic beyond simple CRUD. Covers tactical DDD patterns (entities, value objects, aggregates, factories), DTO boundaries, Result<T> error handling, structured logging with Serilog.
---

# .NET Tactical Domain-Driven Design

## Overview

**Rich domain models with behavior, not anemic data containers.** Apply tactical DDD patterns to all .NET backend development for maintainable, testable, domain-centric code.

**Core principle:** Domain models encapsulate business rules and invariants. DTOs are immutable data contracts at API boundaries. Manual mapping via extension methods. Never expose domain models in APIs.

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

**What:** Immutable objects identified by values, not identity.

**When:** Measurements, money, quantities, descriptive characteristics.

```csharp
// ✅ Good: Immutable with factory validation
public class Money : ValueObject
{
    public decimal Amount { get; } // Readonly
    public string Currency { get; }

    private Money(decimal amount, string currency) { // Private
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
        yield return Amount; // Value equality
        yield return Currency;
    }
}

// ❌ Bad: Mutable, no validation
public class Money {
    public decimal Amount { get; set; } // Public setter
    public string Currency { get; set; }
}
```

## 2. Entities & Aggregates

**Entities:** Objects with unique identity. **Aggregates:** Transaction boundaries.

```csharp
// ✅ Good: Aggregate with behavior and invariants
public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; } // Reference by ID only
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

    public void Complete() {
        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedEvent(Id, GetTotal()));
    }

    public Money GetTotal() => _lines.Aggregate(
        Money.Zero("USD"),
        (sum, line) => sum.Add(line.LineTotal));
}

// ❌ Bad: Anemic entity with public setters
public class Order {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; } // Public setter bypasses validation
    public List<OrderLine> Lines { get; set; } // Exposed list
}
```

**Aggregate Rules:**
- Reference other aggregates by ID only (never full entity)
- Enforce invariants across all child entities
- Single entry point for modifications

## 3. Factories

**Always use factory methods for domain object creation.**

```csharp
// ✅ Good: Factory with validation returns Result<T>
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

    public Result UpdateName(string name) {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Name required");
        Name = name;
        return Result.Success();
    }
}

// ❌ Bad: Public constructor allows invalid state
public class Product {
    public string Name { get; set; }
    public Product() { } // Allows: new Product() with Name = null
}
```

## 4. Repositories

**Abstraction for persisting/retrieving aggregates. Acts like a collection.**

```csharp
// ✅ Good: Specific interface, eager loading, SaveChanges
public interface IOrderRepository {
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default); // ACID transaction
}

public class OrderRepository : IOrderRepository {
    private readonly ApplicationDbContext _context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) {
        return await _context.Orders
            .Include(o => o.Lines) // Eager loading avoids N+1
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct) {
        await _context.Orders.AddAsync(order, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct) {
        await _context.SaveChangesAsync(ct); // Commits transaction
    }
}

// ❌ Bad: Generic repository forcing same interface
public interface IRepository<T> {
    Task<IEnumerable<T>> GetAllAsync(); // Rarely needed
    Task UpdateAsync(T entity); // Often not needed
}
```

## 5. Domain Events

**Immutable records of something that happened (past tense).**

```csharp
// ✅ Good: Immutable, past tense, timestamp
public record OrderShippedEvent(
    Guid OrderId,
    Guid CustomerId,
    DateTime OccurredAt
) : IDomainEvent;

// Aggregate raising event
public class Order : AggregateRoot<Guid> {
    public void Ship() {
        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedEvent(Id, CustomerId, DateTime.UtcNow));
    }
}

// Handler decouples aggregates
public class AwardLoyaltyPointsHandler : IDomainEventHandler<OrderShippedEvent> {
    private readonly ICustomerRepository _customerRepo;

    public async Task Handle(OrderShippedEvent @event, CancellationToken ct) {
        var customer = await _customerRepo.GetByIdAsync(@event.CustomerId, ct);
        if (customer == null) return;

        customer.AwardPoints(100);
        await _customerRepo.SaveChangesAsync(ct);
    }
}

// ❌ Bad: Mutable, present tense
public class ShipOrder { // Should be "OrderShipped"
    public Guid OrderId { get; set; } // Mutable
}
```

## 6. DTOs at API Boundaries

**Always use DTOs. Never expose domain models in controllers.**

```csharp
// ✅ Good: Immutable records for DTOs
public record CreateProductRequest(string Name, decimal Price, string Currency);
public record UpdateProductRequest(Guid Id, string Name);
public record ProductDto(Guid Id, string Name, decimal Price, string Currency);

// ✅ Good: Extension methods for mapping
public static class ProductMapper
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Name,
        product.Price.Amount,
        product.Price.Currency
    );

    public static Result<Product> ToDomain(this CreateProductRequest request)
    {
        var priceResult = Money.Create(request.Price, request.Currency);
        if (priceResult.IsFailure) return Result.Failure<Product>(priceResult.Error);

        return Product.Create(request.Name, priceResult.Value);
    }

    public static Result UpdateFromRequest(this Product product, UpdateProductRequest request)
    {
        return product.UpdateName(request.Name);
    }
}

// ✅ Good: Controller with DTOs and Result<T>
[HttpPost]
public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
{
    var result = request.ToDomain();
    if (result.IsFailure) return BadRequest(result.Error);

    var product = result.Value;
    await _repository.AddAsync(product);
    await _repository.SaveChangesAsync();

    return CreatedAtAction(nameof(Get), new { id = product.Id }, product.ToDto());
}

// ❌ Bad: Exposing domain model
[HttpPost]
public async Task<ActionResult<Product>> Create(Product product) { // Domain in API
    await _repository.AddAsync(product);
    return Ok(product);
}

// ❌ Bad: Inline mapping
[HttpPost]
public async Task<IActionResult> Create(CreateRequest req) {
    var product = new Product { Name = req.Name }; // Direct instantiation
    return Ok(new ProductDto { Id = product.Id }); // Inline mapping
}
```

**DTO Security:**
```csharp
// ❌ Bad: Over-posting attack
public class User {
    public string Name { get; set; }
    public bool IsAdmin { get; set; } // Client can set this
}

[HttpPost]
public async Task<IActionResult> Create(User user) { // Dangerous
    await _repo.AddAsync(user);
    return Ok();
}

// ✅ Good: Request DTO only exposes safe fields
public record CreateUserRequest(string Name);

[HttpPost]
public async Task<ActionResult<UserDto>> Create(CreateUserRequest request) {
    var result = User.Create(request.Name); // IsAdmin controlled by domain
    if (result.IsFailure) return BadRequest(result.Error);

    await _repo.AddAsync(result.Value);
    await _repo.SaveChangesAsync();
    return Ok(result.Value.ToDto());
}
```

**DTO Pagination:**
```csharp
// ✅ Good: Query DTO and paginated response
public record ProductQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 20);
public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductQuery query)
{
    var (products, total) = await _repo.GetPagedAsync(
        query.SearchTerm, query.PageNumber, query.PageSize);

    var dtos = products.Select(p => p.ToDto()).ToList();
    return Ok(new PagedResult<ProductDto>(dtos, total, query.PageNumber, query.PageSize));
}
```

## 7. Structured Logging with Serilog

**Configure Serilog for structured, queryable logs.**

```csharp
// Program.cs - Configure Serilog
using Serilog;
using Serilog.Events;

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            "logs/app-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

    if (context.HostingEnvironment.IsProduction())
    {
        configuration.WriteTo.Seq("http://seq:5341"); // Centralized logging
    }
});

// ✅ Good: Structured logging with properties
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public async Task<Result<Product>> CreateAsync(CreateProductRequest request)
    {
        _logger.LogInformation(
            "Creating product {ProductName} with price {Price} {Currency}",
            request.Name, request.Price, request.Currency);

        var result = request.ToDomain();
        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Product creation failed: {Error}",
                result.Error);
            return result;
        }

        var product = result.Value;
        _logger.LogInformation("Product {ProductId} created successfully", product.Id);

        return Result.Success(product);
    }
}

// ❌ Bad: String interpolation loses structure
_logger.LogInformation($"Creating product {request.Name}"); // Not queryable
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
