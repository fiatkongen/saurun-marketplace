using Domain;
using Xunit;

namespace Tests;

public class OrderTests
{
    private static readonly Guid ValidOrderId = Guid.NewGuid();
    private static readonly Guid ProductA = Guid.NewGuid();
    private static readonly Guid ProductB = Guid.NewGuid();
    private static readonly Guid ProductC = Guid.NewGuid();

    #region Creation

    [Fact]
    public void Create_WithValidInputs_Succeeds()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 2, 25.00m);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidOrderId, result.Value.Id);
        Assert.Equal(OrderStatus.Draft, result.Value.Status);
        Assert.Equal(Currency.USD, result.Value.Currency);
        Assert.Single(result.Value.LineItems);
    }

    [Fact]
    public void Create_WithEmptyOrderId_Fails()
    {
        var result = Order.Create(Guid.Empty, Currency.USD, ProductA, 1, 10.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("OrderId", result.Error!);
    }

    [Fact]
    public void Create_WithZeroQuantity_Fails()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 0, 10.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("at least 1", result.Error!);
    }

    [Fact]
    public void Create_WithNegativeQuantity_Fails()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, -1, 10.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("at least 1", result.Error!);
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_Fails()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, -5.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WithEmptyProductId_Fails()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, Guid.Empty, 1, 10.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("ProductId", result.Error!);
    }

    [Fact]
    public void Create_WithZeroUnitPrice_Succeeds()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 0m);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ExceedingMaxOrderValue_Fails()
    {
        // 1 item at $10,001 exceeds the $10,000 limit
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10_001m);

        Assert.True(result.IsFailure);
        Assert.Contains("exceeding", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_AtExactlyMaxOrderValue_Succeeds()
    {
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10_000m);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_QuantityTimesPrice_ExceedsMax_Fails()
    {
        // 100 * $101 = $10,100 > $10,000
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 100, 101m);

        Assert.True(result.IsFailure);
        Assert.Contains("exceeding", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region AddLineItem

    [Fact]
    public void AddLineItem_ToDraftOrder_Succeeds()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10.00m).Value;

        var result = order.AddLineItem(ProductB, 2, 20.00m);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, order.LineItems.Count);
    }

    [Fact]
    public void AddLineItem_WithZeroQuantity_Fails()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10.00m).Value;

        var result = order.AddLineItem(ProductB, 0, 20.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("at least 1", result.Error!);
    }

    [Fact]
    public void AddLineItem_WithNegativePrice_Fails()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10.00m).Value;

        var result = order.AddLineItem(ProductB, 1, -5.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddLineItem_ExceedingMaxTotal_Fails()
    {
        // Start with $9,000, try to add $1,500
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 9_000m).Value;

        var result = order.AddLineItem(ProductB, 1, 1_500m);

        Assert.True(result.IsFailure);
        Assert.Contains("exceeding", result.Error!, StringComparison.OrdinalIgnoreCase);
        // Ensure line item was NOT added
        Assert.Single(order.LineItems);
    }

    [Fact]
    public void AddLineItem_ReachingExactlyMaxTotal_Succeeds()
    {
        // Start with $9,000, add exactly $1,000 to hit $10,000
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 9_000m).Value;

        var result = order.AddLineItem(ProductB, 1, 1_000m);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, order.LineItems.Count);
    }

    [Fact]
    public void AddLineItem_MultipleItems_AccumulatesCorrectly()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 100m).Value;
        order.AddLineItem(ProductB, 2, 200m);   // subtotal = 100 + 400 = 500
        order.AddLineItem(ProductC, 5, 50m);    // subtotal = 500 + 250 = 750

        var totalResult = order.CalculateTotal();

        Assert.True(totalResult.IsSuccess);
        Assert.Equal(750m, totalResult.Value.Amount);
        Assert.Equal(3, order.LineItems.Count);
    }

    [Fact]
    public void AddLineItem_WithEmptyProductId_Fails()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10.00m).Value;

        var result = order.AddLineItem(Guid.Empty, 1, 5.00m);

        Assert.True(result.IsFailure);
        Assert.Contains("ProductId", result.Error!);
    }

    #endregion

    #region Confirm

    [Fact]
    public void Confirm_DraftOrder_Succeeds()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 50m).Value;

        var result = order.Confirm();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_AlreadyConfirmedOrder_Fails()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 50m).Value;
        order.Confirm();

        var result = order.Confirm();

        Assert.True(result.IsFailure);
        Assert.Contains("already confirmed", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddLineItem_ToConfirmedOrder_Fails()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 50m).Value;
        order.Confirm();

        var result = order.AddLineItem(ProductB, 1, 25m);

        Assert.True(result.IsFailure);
        Assert.Contains("confirmed", result.Error!, StringComparison.OrdinalIgnoreCase);
        Assert.Single(order.LineItems); // item was NOT added
    }

    #endregion

    #region CalculateTotal

    [Fact]
    public void CalculateTotal_SingleItem_ReturnsLineTotal()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 3, 33.33m).Value;

        var result = order.CalculateTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void CalculateTotal_MultipleItems_SumsCorrectly()
    {
        var order = Order.Create(ValidOrderId, Currency.EUR, ProductA, 2, 100m).Value;
        order.AddLineItem(ProductB, 3, 50m);

        var result = order.CalculateTotal();

        Assert.True(result.IsSuccess);
        Assert.Equal(350m, result.Value.Amount); // 200 + 150
        Assert.Equal(Currency.EUR, result.Value.Currency);
    }

    #endregion

    #region Encapsulation

    [Fact]
    public void LineItems_ReturnsReadOnlyList_CannotBeModifiedExternally()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10m).Value;

        var lineItems = order.LineItems;

        // IReadOnlyList does not expose Add; this verifies the type.
        Assert.IsAssignableFrom<IReadOnlyList<OrderLineItem>>(lineItems);
    }

    [Fact]
    public void Create_AlwaysStartsAsDraft()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 10m).Value;

        Assert.Equal(OrderStatus.Draft, order.Status);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AddLineItem_JustOverMax_ByOneCent_Fails()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 9_999.99m).Value;

        var result = order.AddLineItem(ProductB, 1, 0.02m);

        Assert.True(result.IsFailure);
        Assert.Contains("exceeding", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddLineItem_ExactlyAtMax_AfterMultipleAdds_Succeeds()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 3_000m).Value;
        order.AddLineItem(ProductB, 1, 3_000m);

        var result = order.AddLineItem(ProductC, 1, 4_000m);

        Assert.True(result.IsSuccess);

        var total = order.CalculateTotal();
        Assert.Equal(10_000m, total.Value.Amount);
    }

    [Fact]
    public void Create_LargeQuantity_SmallPrice_ExceedingMax_Fails()
    {
        // 10001 * $1.00 = $10,001
        var result = Order.Create(ValidOrderId, Currency.USD, ProductA, 10_001, 1m);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AddLineItem_ZeroPrice_DoesNotAffectTotal()
    {
        var order = Order.Create(ValidOrderId, Currency.USD, ProductA, 1, 5_000m).Value;

        var result = order.AddLineItem(ProductB, 100, 0m);

        Assert.True(result.IsSuccess);
        var total = order.CalculateTotal();
        Assert.Equal(5_000m, total.Value.Amount);
    }

    #endregion
}
