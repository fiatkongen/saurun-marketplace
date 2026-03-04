using Domain.Entities;
using Domain.ValueObjects;

namespace Tests.Unit;

public class OrderTests
{
    private static Guid ValidCustomerId => Guid.NewGuid();

    private static (Guid productId, Quantity quantity, Money unitPrice) ValidLineItem(
        decimal price = 10m, int qty = 1)
    {
        return (
            Guid.NewGuid(),
            Quantity.Create(qty).Value,
            Money.Create(price, "USD").Value
        );
    }

    // --- Order Creation ---

    [Fact]
    public void Create_WithValidCustomerIdAndLineItem_ReturnsSuccess()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();

        var result = Order.Create(ValidCustomerId, productId, quantity, unitPrice);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.LineItems);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ReturnsFailure()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();

        var result = Order.Create(Guid.Empty, productId, quantity, unitPrice);

        Assert.True(result.IsFailure);
        Assert.Contains("customer", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_SetsStatusToPending()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();

        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Create_RaisesOrderCreatedDomainEvent()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();

        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        Assert.Single(order.DomainEvents);
    }

    // --- Adding Line Items ---

    [Fact]
    public void AddLineItem_WithValidInputs_AddsToLineItems()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        var (productId2, quantity2, unitPrice2) = ValidLineItem(20m, 2);
        var result = order.AddLineItem(productId2, quantity2, unitPrice2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, order.LineItems.Count);
    }

    [Fact]
    public void AddLineItem_WithEmptyProductId_ReturnsFailure()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        var qty = Quantity.Create(1).Value;
        var price = Money.Create(10m, "USD").Value;
        var result = order.AddLineItem(Guid.Empty, qty, price);

        Assert.True(result.IsFailure);
    }

    // --- Max Order Value ---

    [Fact]
    public void AddLineItem_ExceedingMaxOrderValue_ReturnsFailure()
    {
        var (productId, quantity, unitPrice) = ValidLineItem(5000m, 1);
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        var (productId2, quantity2, unitPrice2) = ValidLineItem(5001m, 1);
        var result = order.AddLineItem(productId2, quantity2, unitPrice2);

        Assert.True(result.IsFailure);
        Assert.Contains("10,000", result.Error);
    }

    [Fact]
    public void Create_WithLineItemExceedingMaxOrderValue_ReturnsFailure()
    {
        var quantity = Quantity.Create(1).Value;
        var unitPrice = Money.Create(10001m, "USD").Value;

        var result = Order.Create(ValidCustomerId, Guid.NewGuid(), quantity, unitPrice);

        Assert.True(result.IsFailure);
        Assert.Contains("10,000", result.Error);
    }

    [Fact]
    public void AddLineItem_ExactlyAtMaxOrderValue_Succeeds()
    {
        var (productId, quantity, unitPrice) = ValidLineItem(5000m, 1);
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        var (productId2, quantity2, unitPrice2) = ValidLineItem(5000m, 1);
        var result = order.AddLineItem(productId2, quantity2, unitPrice2);

        Assert.True(result.IsSuccess);
    }

    // --- Confirmed Order ---

    [Fact]
    public void Confirm_PendingOrder_SetsStatusToConfirmed()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        var result = order.Confirm();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_RaisesOrderConfirmedDomainEvent()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;
        order.ClearDomainEvents(); // Clear creation event

        order.Confirm();

        Assert.Single(order.DomainEvents);
    }

    [Fact]
    public void AddLineItem_WhenConfirmed_ReturnsFailure()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;
        order.Confirm();

        var (productId2, quantity2, unitPrice2) = ValidLineItem();
        var result = order.AddLineItem(productId2, quantity2, unitPrice2);

        Assert.True(result.IsFailure);
        Assert.Contains("confirmed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ReturnsFailure()
    {
        var (productId, quantity, unitPrice) = ValidLineItem();
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;
        order.Confirm();

        var result = order.Confirm();

        Assert.True(result.IsFailure);
        Assert.Contains("already", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // --- Total Calculation ---

    [Fact]
    public void TotalAmount_WithMultipleLineItems_ReturnsSumOfLineTotals()
    {
        var (productId1, quantity1, unitPrice1) = ValidLineItem(10m, 2); // 20
        var order = Order.Create(ValidCustomerId, productId1, quantity1, unitPrice1).Value;

        var (productId2, quantity2, unitPrice2) = ValidLineItem(15m, 3); // 45
        order.AddLineItem(productId2, quantity2, unitPrice2);

        Assert.Equal(65m, order.TotalAmount.Amount);
        Assert.Equal("USD", order.TotalAmount.Currency);
    }

    [Fact]
    public void TotalAmount_WithSingleLineItem_ReturnsLineTotal()
    {
        var (productId, quantity, unitPrice) = ValidLineItem(25m, 4); // 100
        var order = Order.Create(ValidCustomerId, productId, quantity, unitPrice).Value;

        Assert.Equal(100m, order.TotalAmount.Amount);
    }
}
