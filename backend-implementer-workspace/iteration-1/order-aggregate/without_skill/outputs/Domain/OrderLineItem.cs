namespace Domain;

public sealed class OrderLineItem
{
    public Guid Id { get; }
    public Guid ProductId { get; }
    public Quantity Quantity { get; }
    public Money UnitPrice { get; }

    private OrderLineItem(Guid id, Guid productId, Quantity quantity, Money unitPrice)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<OrderLineItem> Create(Guid productId, Quantity quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
            return Result<OrderLineItem>.Failure("ProductId cannot be empty.");

        return Result<OrderLineItem>.Success(
            new OrderLineItem(Guid.NewGuid(), productId, quantity, unitPrice));
    }

    public Result<Money> LineTotal()
    {
        return UnitPrice.Multiply(Quantity.Value);
    }
}
