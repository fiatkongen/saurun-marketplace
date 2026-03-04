using Domain;
using Xunit;

namespace Tests;

public class OrderLineItemTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var productId = Guid.NewGuid();
        var quantity = Quantity.Create(2).Value!;
        var unitPrice = Money.Create(25m, Currency.USD).Value!;

        var result = OrderLineItem.Create(productId, quantity, unitPrice);

        Assert.True(result.IsSuccess);
        Assert.Equal(productId, result.Value!.ProductId);
        Assert.Equal(quantity, result.Value.Quantity);
        Assert.Equal(unitPrice, result.Value.UnitPrice);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void Create_WithEmptyProductId_ReturnsFailure()
    {
        var quantity = Quantity.Create(1).Value!;
        var unitPrice = Money.Create(10m, Currency.USD).Value!;

        var result = OrderLineItem.Create(Guid.Empty, quantity, unitPrice);

        Assert.True(result.IsFailure);
        Assert.Contains("ProductId", result.Error!);
    }

    [Fact]
    public void LineTotal_ReturnsQuantityTimesUnitPrice()
    {
        var quantity = Quantity.Create(3).Value!;
        var unitPrice = Money.Create(15.50m, Currency.USD).Value!;
        var lineItem = OrderLineItem.Create(Guid.NewGuid(), quantity, unitPrice).Value!;

        var result = lineItem.LineTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(46.50m, result.Value!.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void LineTotal_SingleQuantity_EqualsUnitPrice()
    {
        var quantity = Quantity.Create(1).Value!;
        var unitPrice = Money.Create(99.99m, Currency.EUR).Value!;
        var lineItem = OrderLineItem.Create(Guid.NewGuid(), quantity, unitPrice).Value!;

        var result = lineItem.LineTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value!.Amount);
    }
}
