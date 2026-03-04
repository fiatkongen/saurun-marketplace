using Domain.ValueObjects;

namespace Tests.Unit;

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
    public void Equality_SameValue_AreEqual()
    {
        var q1 = Quantity.Create(3).Value;
        var q2 = Quantity.Create(3).Value;

        Assert.Equal(q1, q2);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        var q1 = Quantity.Create(3).Value;
        var q2 = Quantity.Create(5).Value;

        Assert.NotEqual(q1, q2);
    }
}
