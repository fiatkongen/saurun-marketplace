using OrderAggregate.Domain;
using OrderAggregate.Domain.Entities;
using OrderAggregate.Domain.Events;
using OrderAggregate.Domain.ValueObjects;

namespace OrderAggregate.Tests.Unit;

public class OrderTests
{
    private static readonly Guid ValidProductId = Guid.NewGuid();
    private static readonly Guid ValidCustomerId = Guid.NewGuid();

    // --- Factory / Creation ---

    [Fact]
    public void Create_WithValidCustomerId_ReturnsSuccessWithPendingStatus()
    {
        var result = Order.Create(ValidCustomerId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidCustomerId, result.Value.CustomerId);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
        Assert.Empty(result.Value.LineItems);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ReturnsFailure()
    {
        var result = Order.Create(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Contains("customer", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_RaisesOrderCreatedEvent()
    {
        var result = Order.Create(ValidCustomerId);

        Assert.True(result.IsSuccess);
        var domainEvent = Assert.Single(result.Value.DomainEvents);
        var orderCreated = Assert.IsType<OrderCreatedEvent>(domainEvent);
        Assert.Equal(result.Value.Id, orderCreated.OrderId);
        Assert.Equal(ValidCustomerId, orderCreated.CustomerId);
    }

    // --- AddLineItem ---

    [Fact]
    public void AddLineItem_WithValidInputs_AddsItemToOrder()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var quantity = Quantity.Create(2).Value;
        var unitPrice = Money.Create(25.00m, "USD").Value;

        var result = order.AddLineItem(ValidProductId, quantity, unitPrice);

        Assert.True(result.IsSuccess);
        Assert.Single(order.LineItems);
        Assert.Equal(ValidProductId, order.LineItems[0].ProductId);
    }

    [Fact]
    public void AddLineItem_MultipleItems_AddsAllItems()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var qty = Quantity.Create(1).Value;
        var price = Money.Create(10.00m, "USD").Value;

        order.AddLineItem(Guid.NewGuid(), qty, price);
        order.AddLineItem(Guid.NewGuid(), qty, price);
        order.AddLineItem(Guid.NewGuid(), qty, price);

        Assert.Equal(3, order.LineItems.Count);
    }

    // --- Total order value cannot exceed $10,000 ---

    [Fact]
    public void AddLineItem_WhenTotalWouldExceed10000_ReturnsFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var qty = Quantity.Create(1).Value;
        var price = Money.Create(9999.00m, "USD").Value;
        order.AddLineItem(ValidProductId, qty, price);

        var overflowPrice = Money.Create(1.01m, "USD").Value;
        var result = order.AddLineItem(Guid.NewGuid(), qty, overflowPrice);

        Assert.True(result.IsFailure);
        Assert.Contains("10,000", result.Error);
    }

    [Fact]
    public void AddLineItem_WhenTotalExactly10000_ReturnsSuccess()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var qty = Quantity.Create(1).Value;
        var price = Money.Create(10_000.00m, "USD").Value;

        var result = order.AddLineItem(ValidProductId, qty, price);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AddLineItem_WhenTotalWouldExceed10000WithQuantity_ReturnsFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var qty = Quantity.Create(101).Value;
        var price = Money.Create(100.00m, "USD").Value;

        var result = order.AddLineItem(ValidProductId, qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("10,000", result.Error);
    }

    // --- Once confirmed, no more items can be added ---

    [Fact]
    public void AddLineItem_WhenOrderConfirmed_ReturnsFailure()
    {
        var order = CreateOrderWithOneItem();
        order.Confirm();

        var qty = Quantity.Create(1).Value;
        var price = Money.Create(5.00m, "USD").Value;
        var result = order.AddLineItem(Guid.NewGuid(), qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("confirmed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // --- Confirm ---

    [Fact]
    public void Confirm_WithAtLeastOneItem_ReturnsSuccessAndSetsConfirmedStatus()
    {
        var order = CreateOrderWithOneItem();

        var result = order.Confirm();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WithNoItems_ReturnsFailure()
    {
        var order = Order.Create(ValidCustomerId).Value;

        var result = order.Confirm();

        Assert.True(result.IsFailure);
        Assert.Contains("at least one", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ReturnsFailure()
    {
        var order = CreateOrderWithOneItem();
        order.Confirm();

        var result = order.Confirm();

        Assert.True(result.IsFailure);
        Assert.Contains("already", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Confirm_RaisesOrderConfirmedEvent()
    {
        var order = CreateOrderWithOneItem();
        order.ClearDomainEvents(); // Clear the OrderCreatedEvent

        order.Confirm();

        var domainEvent = Assert.Single(order.DomainEvents);
        var confirmedEvent = Assert.IsType<OrderConfirmedEvent>(domainEvent);
        Assert.Equal(order.Id, confirmedEvent.OrderId);
    }

    // --- TotalAmount ---

    [Fact]
    public void TotalAmount_WithMultipleItems_ReturnsSumOfLineTotals()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var qty1 = Quantity.Create(2).Value;
        var price1 = Money.Create(10.00m, "USD").Value;
        order.AddLineItem(Guid.NewGuid(), qty1, price1); // 20.00

        var qty2 = Quantity.Create(3).Value;
        var price2 = Money.Create(15.00m, "USD").Value;
        order.AddLineItem(Guid.NewGuid(), qty2, price2); // 45.00

        var total = order.TotalAmount;
        Assert.Equal(65.00m, total.Amount);
        Assert.Equal("USD", total.Currency);
    }

    [Fact]
    public void TotalAmount_WithNoItems_ReturnsZero()
    {
        var order = Order.Create(ValidCustomerId).Value;

        var total = order.TotalAmount;
        Assert.Equal(0m, total.Amount);
        Assert.Equal("USD", total.Currency);
    }

    // --- Helper ---

    private static Order CreateOrderWithOneItem()
    {
        var order = Order.Create(ValidCustomerId).Value;
        var qty = Quantity.Create(1).Value;
        var price = Money.Create(50.00m, "USD").Value;
        order.AddLineItem(ValidProductId, qty, price);
        return order;
    }
}
