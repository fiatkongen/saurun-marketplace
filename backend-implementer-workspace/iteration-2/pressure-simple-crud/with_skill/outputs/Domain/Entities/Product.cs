using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public Quantity StockCount { get; private set; }

    private Product(Guid id, string name, Money price, Quantity stockCount) : base(id)
    {
        Name = name;
        Price = price;
        StockCount = stockCount;
    }

    // EF Core
    private Product() : base()
    {
        Name = string.Empty;
        Price = null!;
        StockCount = null!;
    }

    public static Result<Product> Create(string name, Money price, Quantity stockCount)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>("Name is required");
        if (name.Length > 200)
            return Result.Failure<Product>("Name is too long (max 200 characters)");
        if (price.Amount <= 0)
            return Result.Failure<Product>("Price must be positive");

        var product = new Product(Guid.NewGuid(), name, price, stockCount);
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, name, DateTime.UtcNow));
        return Result.Success(product);
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Name is required");
        if (name.Length > 200)
            return Result.Failure("Name is too long (max 200 characters)");

        Name = name;
        AddDomainEvent(new ProductUpdatedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }

    public Result UpdatePrice(Money price)
    {
        if (price.Amount <= 0)
            return Result.Failure("Price must be positive");

        Price = price;
        AddDomainEvent(new ProductUpdatedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }

    public Result UpdateStock(Quantity stockCount)
    {
        StockCount = stockCount;
        AddDomainEvent(new ProductUpdatedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }
}
