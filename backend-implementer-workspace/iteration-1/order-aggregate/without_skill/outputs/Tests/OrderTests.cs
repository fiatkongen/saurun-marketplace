using Domain;
using Xunit;

namespace Tests;

public class OrderTests
{
    private static Guid ProductId() => Guid.NewGuid();

    private static Order CreateValidOrder(
        decimal unitPrice = 100m,
        int quantity = 1,
        Currency currency = Currency.USD)
    {
        var qty = Quantity.Create(quantity).Value!;
        var price = Money.Create(unitPrice, currency).Value!;
        return Order.Create(currency, ProductId(), qty, price).Value!;
    }

    #region Order Creation

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var qty = Quantity.Create(2).Value!;
        var price = Money.Create(50m, Currency.USD).Value!;

        var result = Order.Create(Currency.USD, ProductId(), qty, price);

        Assert.True(result.IsSuccess);
        var order = result.Value!;
        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Equal(Currency.USD, order.Currency);
        Assert.Single(order.LineItems);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Fact]
    public void Create_CurrencyMismatch_ReturnsFailure()
    {
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(50m, Currency.EUR).Value!;

        var result = Order.Create(Currency.USD, ProductId(), qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("currency", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ExceedingMaxValue_ReturnsFailure()
    {
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(10_001m, Currency.USD).Value!;

        var result = Order.Create(Currency.USD, ProductId(), qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("exceeding", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ExactlyAtMaxValue_ReturnsSuccess()
    {
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(10_000m, Currency.USD).Value!;

        var result = Order.Create(Currency.USD, ProductId(), qty, price);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region AddLineItem

    [Fact]
    public void AddLineItem_ToDraftOrder_ReturnsSuccess()
    {
        var order = CreateValidOrder(100m);
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(50m, Currency.USD).Value!;

        var result = order.AddLineItem(ProductId(), qty, price);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, order.LineItems.Count);
    }

    [Fact]
    public void AddLineItem_ToConfirmedOrder_ReturnsFailure()
    {
        var order = CreateValidOrder(100m);
        order.Confirm();

        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(50m, Currency.USD).Value!;

        var result = order.AddLineItem(ProductId(), qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("confirmed", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddLineItem_CurrencyMismatch_ReturnsFailure()
    {
        var order = CreateValidOrder(100m, currency: Currency.USD);
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(50m, Currency.EUR).Value!;

        var result = order.AddLineItem(ProductId(), qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("currency", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddLineItem_ExceedsMaxValue_ReturnsFailure()
    {
        var order = CreateValidOrder(9_000m);
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(1_001m, Currency.USD).Value!;

        var result = order.AddLineItem(ProductId(), qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("exceeding", result.Error!, StringComparison.OrdinalIgnoreCase);
        // Original order unchanged
        Assert.Single(order.LineItems);
    }

    [Fact]
    public void AddLineItem_BringsToExactlyMaxValue_ReturnsSuccess()
    {
        var order = CreateValidOrder(9_000m);
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(1_000m, Currency.USD).Value!;

        var result = order.AddLineItem(ProductId(), qty, price);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, order.LineItems.Count);
    }

    [Fact]
    public void AddLineItem_WithEmptyProductId_ReturnsFailure()
    {
        var order = CreateValidOrder(100m);
        var qty = Quantity.Create(1).Value!;
        var price = Money.Create(50m, Currency.USD).Value!;

        var result = order.AddLineItem(Guid.Empty, qty, price);

        Assert.True(result.IsFailure);
        Assert.Contains("ProductId", result.Error!);
    }

    [Fact]
    public void AddLineItem_MultipleItems_AccumulatesTotal()
    {
        var order = CreateValidOrder(2_000m, quantity: 2); // 2 x 2000 = 4000
        var qty = Quantity.Create(3).Value!;
        var price = Money.Create(1_000m, Currency.USD).Value!;

        // Adding 3 x 1000 = 3000, total = 7000
        var result = order.AddLineItem(ProductId(), qty, price);
        Assert.True(result.IsSuccess);

        var total = order.CalculateTotal().Value!;
        Assert.Equal(7_000m, total.Amount);
    }

    #endregion

    #region Confirm

    [Fact]
    public void Confirm_DraftOrderWithItems_ReturnsSuccess()
    {
        var order = CreateValidOrder(100m);

        var result = order.Confirm();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ReturnsFailure()
    {
        var order = CreateValidOrder(100m);
        order.Confirm();

        var result = order.Confirm();

        Assert.True(result.IsFailure);
        Assert.Contains("already confirmed", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region CalculateTotal

    [Fact]
    public void CalculateTotal_SingleItem_ReturnsCorrectTotal()
    {
        var order = CreateValidOrder(25.50m, quantity: 4);

        var result = order.CalculateTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(102m, result.Value!.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void CalculateTotal_MultipleItems_SumsAllLineTotals()
    {
        var order = CreateValidOrder(100m, quantity: 2); // 200

        var qty2 = Quantity.Create(3).Value!;
        var price2 = Money.Create(50m, Currency.USD).Value!;
        order.AddLineItem(ProductId(), qty2, price2); // 150

        var qty3 = Quantity.Create(1).Value!;
        var price3 = Money.Create(75m, Currency.USD).Value!;
        order.AddLineItem(ProductId(), qty3, price3); // 75

        var result = order.CalculateTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(425m, result.Value!.Amount);
    }

    #endregion

    #region Encapsulation

    [Fact]
    public void LineItems_ReturnsReadOnlyCopy()
    {
        var order = CreateValidOrder(100m);

        var items = order.LineItems;

        // Should not be castable to mutable list
        Assert.IsNotType<List<OrderLineItem>>(items);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithZeroUnitPrice_ReturnsSuccess()
    {
        var qty = Quantity.Create(5).Value!;
        var price = Money.Create(0m, Currency.USD).Value!;

        var result = Order.Create(Currency.USD, ProductId(), qty, price);

        Assert.True(result.IsSuccess);
        var total = result.Value!.CalculateTotal().Value!;
        Assert.Equal(0m, total.Amount);
    }

    [Fact]
    public void AddLineItem_ManySmallItems_AccumulatesCorrectly()
    {
        var order = CreateValidOrder(1m, quantity: 1); // 1
        for (var i = 0; i < 99; i++)
        {
            var qty = Quantity.Create(1).Value!;
            var price = Money.Create(1m, Currency.USD).Value!;
            var result = order.AddLineItem(ProductId(), qty, price);
            Assert.True(result.IsSuccess);
        }

        Assert.Equal(100, order.LineItems.Count);
        Assert.Equal(100m, order.CalculateTotal().Value!.Amount);
    }

    [Fact]
    public void Order_NonUsdCurrency_WorksCorrectly()
    {
        var qty = Quantity.Create(2).Value!;
        var price = Money.Create(500m, Currency.DKK).Value!;

        var result = Order.Create(Currency.DKK, ProductId(), qty, price);

        Assert.True(result.IsSuccess);
        Assert.Equal(Currency.DKK, result.Value!.Currency);
        Assert.Equal(1_000m, result.Value.CalculateTotal().Value!.Amount);
    }

    #endregion
}
