namespace Domain;

public sealed class Order
{
    private const decimal MaxOrderValue = 10_000m;

    private readonly List<OrderLineItem> _lineItems = [];

    public Guid Id { get; }
    public OrderStatus Status { get; private set; }
    public Currency Currency { get; }
    public IReadOnlyList<OrderLineItem> LineItems => _lineItems.AsReadOnly();

    private Order(Guid id, Currency currency)
    {
        Id = id;
        Status = OrderStatus.Draft;
        Currency = currency;
    }

    public static Result<Order> Create(Currency currency, Guid productId, Quantity quantity, Money unitPrice)
    {
        if (unitPrice.Currency != currency)
            return Result<Order>.Failure($"UnitPrice currency ({unitPrice.Currency}) must match order currency ({currency}).");

        var order = new Order(Guid.NewGuid(), currency);

        var addResult = order.AddLineItemInternal(productId, quantity, unitPrice);
        if (addResult.IsFailure)
            return Result<Order>.Failure(addResult.Error!);

        return Result<Order>.Success(order);
    }

    public Result<OrderLineItem> AddLineItem(Guid productId, Quantity quantity, Money unitPrice)
    {
        if (Status == OrderStatus.Confirmed)
            return Result<OrderLineItem>.Failure("Cannot add items to a confirmed order.");

        if (unitPrice.Currency != Currency)
            return Result<OrderLineItem>.Failure($"UnitPrice currency ({unitPrice.Currency}) must match order currency ({Currency}).");

        return AddLineItemInternal(productId, quantity, unitPrice);
    }

    private Result<OrderLineItem> AddLineItemInternal(Guid productId, Quantity quantity, Money unitPrice)
    {
        var lineItemResult = OrderLineItem.Create(productId, quantity, unitPrice);
        if (lineItemResult.IsFailure)
            return lineItemResult;

        var lineItem = lineItemResult.Value!;

        // Check if adding this item would exceed the max order value
        var lineTotalResult = lineItem.LineTotal();
        if (lineTotalResult.IsFailure)
            return Result<OrderLineItem>.Failure(lineTotalResult.Error!);

        var currentTotalResult = CalculateTotal();
        if (currentTotalResult.IsFailure)
            return Result<OrderLineItem>.Failure(currentTotalResult.Error!);

        var newTotalResult = currentTotalResult.Value!.Add(lineTotalResult.Value!);
        if (newTotalResult.IsFailure)
            return Result<OrderLineItem>.Failure(newTotalResult.Error!);

        if (newTotalResult.Value!.Amount > MaxOrderValue)
            return Result<OrderLineItem>.Failure(
                $"Adding this item would bring the order total to {newTotalResult.Value.Amount:F2} {Currency}, exceeding the maximum of {MaxOrderValue:F2} {Currency}.");

        _lineItems.Add(lineItem);
        return Result<OrderLineItem>.Success(lineItem);
    }

    public Result<Order> Confirm()
    {
        if (Status == OrderStatus.Confirmed)
            return Result<Order>.Failure("Order is already confirmed.");

        if (_lineItems.Count == 0)
            return Result<Order>.Failure("Cannot confirm an order with no line items.");

        Status = OrderStatus.Confirmed;
        return Result<Order>.Success(this);
    }

    public Result<Money> CalculateTotal()
    {
        var total = Money.Zero(Currency);

        foreach (var item in _lineItems)
        {
            var lineTotalResult = item.LineTotal();
            if (lineTotalResult.IsFailure)
                return Result<Money>.Failure(lineTotalResult.Error!);

            var addResult = total.Add(lineTotalResult.Value!);
            if (addResult.IsFailure)
                return Result<Money>.Failure(addResult.Error!);

            total = addResult.Value!;
        }

        return Result<Money>.Success(total);
    }
}
