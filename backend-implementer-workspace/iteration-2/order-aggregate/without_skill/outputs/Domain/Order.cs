namespace Domain;

/// <summary>
/// Aggregate root representing a customer order.
/// Enforces invariants:
///   - Must have at least one line item
///   - Total order value cannot exceed $10,000 (in order currency)
///   - Cannot add items once confirmed
/// </summary>
public sealed class Order
{
    private static readonly decimal MaxOrderValue = 10_000m;

    private readonly List<OrderLineItem> _lineItems = [];

    public Guid Id { get; }
    public OrderStatus Status { get; private set; }
    public Currency Currency { get; }
    public IReadOnlyList<OrderLineItem> LineItems => _lineItems.AsReadOnly();

    private Order(Guid id, Currency currency)
    {
        Id = id;
        Currency = currency;
        Status = OrderStatus.Draft;
    }

    /// <summary>
    /// Creates a new draft Order with one initial line item (an order must never be empty).
    /// </summary>
    public static Result<Order> Create(
        Guid orderId,
        Currency currency,
        Guid productId,
        int quantity,
        decimal unitPrice)
    {
        if (orderId == Guid.Empty)
            return Result.Failure<Order>("OrderId cannot be empty.");

        // Validate value objects
        var quantityResult = Quantity.Create(quantity);
        if (quantityResult.IsFailure)
            return Result.Failure<Order>(quantityResult.Error!);

        var moneyResult = Money.Create(unitPrice, currency);
        if (moneyResult.IsFailure)
            return Result.Failure<Order>(moneyResult.Error!);

        var lineItemResult = OrderLineItem.Create(productId, quantityResult.Value, moneyResult.Value);
        if (lineItemResult.IsFailure)
            return Result.Failure<Order>(lineItemResult.Error!);

        var lineItem = lineItemResult.Value;

        // Validate line total against max
        var lineTotalResult = lineItem.LineTotal();
        if (lineTotalResult.IsFailure)
            return Result.Failure<Order>(lineTotalResult.Error!);

        if (lineTotalResult.Value.Amount > MaxOrderValue)
            return Result.Failure<Order>(
                $"Order total would be {lineTotalResult.Value.Amount:F2} {currency}, " +
                $"exceeding the maximum of {MaxOrderValue:F2} {currency}.");

        var order = new Order(orderId, currency);
        order._lineItems.Add(lineItem);
        return Result.Success(order);
    }

    /// <summary>
    /// Adds a line item to a draft order. Fails if order is confirmed or if the new total exceeds the limit.
    /// </summary>
    public Result<Order> AddLineItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status == OrderStatus.Confirmed)
            return Result.Failure<Order>("Cannot add items to a confirmed order.");

        var quantityResult = Quantity.Create(quantity);
        if (quantityResult.IsFailure)
            return Result.Failure<Order>(quantityResult.Error!);

        var moneyResult = Money.Create(unitPrice, Currency);
        if (moneyResult.IsFailure)
            return Result.Failure<Order>(moneyResult.Error!);

        var lineItemResult = OrderLineItem.Create(productId, quantityResult.Value, moneyResult.Value);
        if (lineItemResult.IsFailure)
            return Result.Failure<Order>(lineItemResult.Error!);

        // Calculate prospective total
        var newTotalResult = CalculateTotalWith(lineItemResult.Value);
        if (newTotalResult.IsFailure)
            return Result.Failure<Order>(newTotalResult.Error!);

        if (newTotalResult.Value.Amount > MaxOrderValue)
            return Result.Failure<Order>(
                $"Adding this item would bring the order total to {newTotalResult.Value.Amount:F2} {Currency}, " +
                $"exceeding the maximum of {MaxOrderValue:F2} {Currency}.");

        _lineItems.Add(lineItemResult.Value);
        return Result.Success(this);
    }

    /// <summary>
    /// Confirms the order, preventing further modifications.
    /// </summary>
    public Result<Order> Confirm()
    {
        if (Status == OrderStatus.Confirmed)
            return Result.Failure<Order>("Order is already confirmed.");

        Status = OrderStatus.Confirmed;
        return Result.Success(this);
    }

    /// <summary>
    /// Calculates the current total of all line items.
    /// </summary>
    public Result<Money> CalculateTotal()
    {
        var total = Money.Zero(Currency);

        foreach (var item in _lineItems)
        {
            var lineTotalResult = item.LineTotal();
            if (lineTotalResult.IsFailure)
                return lineTotalResult;

            var addResult = total.Add(lineTotalResult.Value);
            if (addResult.IsFailure)
                return addResult;

            total = addResult.Value;
        }

        return Result.Success(total);
    }

    /// <summary>
    /// Calculates total including a prospective new line item (used for validation before adding).
    /// </summary>
    private Result<Money> CalculateTotalWith(OrderLineItem additionalItem)
    {
        var currentTotalResult = CalculateTotal();
        if (currentTotalResult.IsFailure)
            return currentTotalResult;

        var additionalLineTotalResult = additionalItem.LineTotal();
        if (additionalLineTotalResult.IsFailure)
            return additionalLineTotalResult;

        return currentTotalResult.Value.Add(additionalLineTotalResult.Value);
    }
}
