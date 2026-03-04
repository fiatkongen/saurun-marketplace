namespace Domain;

/// <summary>
/// Entity within the Order aggregate representing a single line item.
/// Identified by its ProductId within the order context.
/// </summary>
public sealed class OrderLineItem
{
    public Guid ProductId { get; }
    public Quantity Quantity { get; }
    public Money UnitPrice { get; }

    private OrderLineItem(Guid productId, Quantity quantity, Money unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Creates an OrderLineItem with validated inputs.
    /// </summary>
    public static Result<OrderLineItem> Create(Guid productId, Quantity quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
            return Result.Failure<OrderLineItem>("ProductId cannot be empty.");

        return Result.Success(new OrderLineItem(productId, quantity, unitPrice));
    }

    /// <summary>
    /// Calculates the line total: UnitPrice * Quantity.
    /// </summary>
    public Result<Money> LineTotal() => UnitPrice.Multiply(Quantity.Value);
}
