using Domain.Base;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Order : AggregateRoot<Guid>
{
    private const decimal MaxOrderValue = 10_000m;

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderLineItem> _lineItems = new();
    public IReadOnlyList<OrderLineItem> LineItems => _lineItems.AsReadOnly();

    public Money TotalAmount => CalculateTotal();

    private Order() { } // EF Core

    private Order(Guid id, Guid customerId) : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
    }

    public static Result<Order> Create(
        Guid customerId,
        Guid firstProductId,
        Quantity firstQuantity,
        Money firstUnitPrice)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>("Customer ID is required");

        var order = new Order(Guid.NewGuid(), customerId);

        var lineItemResult = OrderLineItem.Create(firstProductId, firstQuantity, firstUnitPrice);
        if (lineItemResult.IsFailure)
            return Result.Failure<Order>(lineItemResult.Error);

        // Check max value before adding
        var lineTotal = lineItemResult.Value.LineTotal.Amount;
        if (lineTotal > MaxOrderValue)
            return Result.Failure<Order>($"Total order value cannot exceed $10,000");

        order._lineItems.Add(lineItemResult.Value);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, DateTime.UtcNow));

        return Result.Success(order);
    }

    public Result AddLineItem(Guid productId, Quantity quantity, Money unitPrice)
    {
        if (Status == OrderStatus.Confirmed)
            return Result.Failure("Cannot add items to a confirmed order");

        var lineItemResult = OrderLineItem.Create(productId, quantity, unitPrice);
        if (lineItemResult.IsFailure)
            return Result.Failure(lineItemResult.Error);

        var newLineTotal = lineItemResult.Value.LineTotal.Amount;
        var currentTotal = CalculateTotal().Amount;
        if (currentTotal + newLineTotal > MaxOrderValue)
            return Result.Failure($"Total order value cannot exceed $10,000");

        _lineItems.Add(lineItemResult.Value);
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status == OrderStatus.Confirmed)
            return Result.Failure("Order is already confirmed");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }

    private Money CalculateTotal()
    {
        if (_lineItems.Count == 0)
            return Money.Zero("USD");

        var currency = _lineItems[0].UnitPrice.Currency;
        var total = Money.Zero(currency);

        foreach (var item in _lineItems)
        {
            var addResult = total.Add(item.LineTotal);
            if (addResult.IsSuccess)
                total = addResult.Value;
        }

        return total;
    }
}
