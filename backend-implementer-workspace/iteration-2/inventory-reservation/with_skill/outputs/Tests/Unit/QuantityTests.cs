using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests.Unit;

public class QuantityTests
{
    [Fact]
    public void Create_WithPositiveValue_ReturnsSuccess()
    {
        var result = Quantity.Create(10);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeValue_ReturnsFailure(int value)
    {
        var result = Quantity.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Subtract_WhenSufficient_ReturnsCorrectQuantity()
    {
        var qty = Quantity.Create(10).Value;

        var result = qty.Subtract(Quantity.Create(3).Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.Value);
    }

    [Fact]
    public void Subtract_WhenInsufficient_ReturnsFailure()
    {
        var qty = Quantity.Create(5).Value;

        var result = qty.Subtract(Quantity.Create(10).Value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Add_ReturnsCombinedQuantity()
    {
        var a = Quantity.Create(5).Value;
        var b = Quantity.Create(3).Value;

        var result = a.Add(b);

        Assert.Equal(8, result.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = Quantity.Create(5).Value;
        var b = Quantity.Create(5).Value;

        Assert.Equal(a, b);
    }

    [Fact]
    public void GreaterThanOrEqual_CorrectComparison()
    {
        var five = Quantity.Create(5).Value;
        var three = Quantity.Create(3).Value;

        Assert.True(five >= three);
        Assert.False(three >= five);
    }
}
