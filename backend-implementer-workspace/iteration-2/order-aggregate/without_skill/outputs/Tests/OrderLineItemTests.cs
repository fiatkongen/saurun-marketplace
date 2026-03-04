using Domain;
using Xunit;

namespace Tests;

public class OrderLineItemTests
{
    [Fact]
    public void Create_WithValidInputs_Succeeds()
    {
        var productId = Guid.NewGuid();
        var quantity = Quantity.Create(2).Value;
        var unitPrice = Money.Create(15.00m, Currency.USD).Value;

        var result = OrderLineItem.Create(productId, quantity, unitPrice);

        Assert.True(result.IsSuccess);
        Assert.Equal(productId, result.Value.ProductId);
        Assert.Equal(quantity, result.Value.Quantity);
        Assert.Equal(unitPrice, result.Value.UnitPrice);
    }

    [Fact]
    public void Create_WithEmptyProductId_Fails()
    {
        var quantity = Quantity.Create(1).Value;
        var unitPrice = Money.Create(10.00m, Currency.USD).Value;

        var result = OrderLineItem.Create(Guid.Empty, quantity, unitPrice);

        Assert.True(result.IsFailure);
        Assert.Contains("ProductId", result.Error!);
    }

    [Fact]
    public void LineTotal_CalculatesCorrectly()
    {
        var quantity = Quantity.Create(3).Value;
        var unitPrice = Money.Create(25.00m, Currency.USD).Value;
        var lineItem = OrderLineItem.Create(Guid.NewGuid(), quantity, unitPrice).Value;

        var result = lineItem.LineTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(75.00m, result.Value.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void LineTotal_SingleQuantity_EqualsUnitPrice()
    {
        var quantity = Quantity.Create(1).Value;
        var unitPrice = Money.Create(49.99m, Currency.EUR).Value;
        var lineItem = OrderLineItem.Create(Guid.NewGuid(), quantity, unitPrice).Value;

        var result = lineItem.LineTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(49.99m, result.Value.Amount);
    }
}
