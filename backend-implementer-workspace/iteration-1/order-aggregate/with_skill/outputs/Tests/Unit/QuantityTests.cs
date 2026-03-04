using OrderAggregate.Domain;
using OrderAggregate.Domain.ValueObjects;

namespace OrderAggregate.Tests.Unit;

public class QuantityTests
{
    [Fact]
    public void Create_WithPositiveValue_ReturnsSuccess()
    {
        var result = Quantity.Create(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Value);
    }

    [Fact]
    public void Create_WithOne_ReturnsSuccess()
    {
        var result = Quantity.Create(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegative_ReturnsFailure(int value)
    {
        var result = Quantity.Create(value);

        Assert.True(result.IsFailure);
        Assert.Contains("positive", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var qty1 = Quantity.Create(3).Value;
        var qty2 = Quantity.Create(3).Value;

        Assert.Equal(qty1, qty2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var qty1 = Quantity.Create(3).Value;
        var qty2 = Quantity.Create(5).Value;

        Assert.NotEqual(qty1, qty2);
    }
}
