using OrderAggregate.Domain.Events;
using OrderAggregate.Domain.ValueObjects;

namespace OrderAggregate.Domain.Entities;

public class Order : AggregateRoot<Guid>
{
    private const decimal MaxOrderValue = 10_000m;
    private const string DefaultCurrency = "USD";

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderLineItem> _lineItems = new();
    public IReadOnlyList<OrderLineItem> LineItems => _lineItems.AsReadOnly();

    public Money TotalAmount
    {
        get
        {
            if (_lineItems.Count == 0)
                return Money.Create(0m, DefaultCurrency).Value;

            var total = _lineItems[0].LineTotal;
            for (int i = 1; i < _lineItems.Count; i++)
            {
                total = total.Add(_lineItems[i].LineTotal).Value;
            }
            return total;
        }
    }

    private Order() { } // EF Core

    private Order(Guid id, Guid customerId) : base(id)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
    }

    public static Result<Order> Create(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>("Customer ID is required");

        var order = new Order(Guid.NewGuid(), customerId);
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

        var lineItem = lineItemResult.Value;
        var newLineTotal = unitPrice.Amount * quantity.Value;
        var currentTotal = TotalAmount.Amount;

        if (currentTotal + newLineTotal > MaxOrderValue)
            return Result.Failure("Total order value cannot exceed $10,000");

        _lineItems.Add(lineItem);
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status == OrderStatus.Confirmed)
            return Result.Failure("Order is already confirmed");

        if (_lineItems.Count == 0)
            return Result.Failure("Order must have at least one line item");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }
}
