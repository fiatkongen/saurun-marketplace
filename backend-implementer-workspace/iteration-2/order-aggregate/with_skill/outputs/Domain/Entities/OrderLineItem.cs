using Domain.Base;
using Domain.ValueObjects;

namespace Domain.Entities;

public class OrderLineItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;

    public Money LineTotal => UnitPrice.Multiply(Quantity.Value);

    private OrderLineItem() { } // EF Core

    private OrderLineItem(Guid id, Guid productId, Quantity quantity, Money unitPrice)
        : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<OrderLineItem> Create(Guid productId, Quantity quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
            return Result.Failure<OrderLineItem>("Product ID is required");

        return Result.Success(new OrderLineItem(Guid.NewGuid(), productId, quantity, unitPrice));
    }
}
