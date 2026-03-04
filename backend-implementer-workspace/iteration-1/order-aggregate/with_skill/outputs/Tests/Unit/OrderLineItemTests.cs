using OrderAggregate.Domain;
using OrderAggregate.Domain.Entities;
using OrderAggregate.Domain.ValueObjects;

namespace OrderAggregate.Tests.Unit;

public class OrderLineItemTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var quantity = Quantity.Create(2).Value;
        var unitPrice = Money.Create(25.00m, "USD").Value;

        var result = OrderLineItem.Create(productId, quantity, unitPrice);

        Assert.True(result.IsSuccess);
        Assert.Equal(productId, result.Value.ProductId);
        Assert.Equal(quantity, result.Value.Quantity);
        Assert.Equal(unitPrice, result.Value.UnitPrice);
    }

    [Fact]
    public void Create_WithEmptyProductId_ReturnsFailure()
    {
        var quantity = Quantity.Create(2).Value;
        var unitPrice = Money.Create(25.00m, "USD").Value;

        var result = OrderLineItem.Create(Guid.Empty, quantity, unitPrice);

        Assert.True(result.IsFailure);
        Assert.Contains("product", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LineTotal_ReturnsQuantityTimesUnitPrice()
    {
        var quantity = Quantity.Create(3).Value;
        var unitPrice = Money.Create(10.00m, "USD").Value;

        var lineItem = OrderLineItem.Create(Guid.NewGuid(), quantity, unitPrice).Value;

        var total = lineItem.LineTotal;
        Assert.Equal(30.00m, total.Amount);
        Assert.Equal("USD", total.Currency);
    }
}
