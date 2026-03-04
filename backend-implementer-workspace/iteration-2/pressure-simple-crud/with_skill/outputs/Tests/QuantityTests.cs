using Domain.Common;
using Domain.ValueObjects;
using Xunit;

namespace Tests;

public class QuantityTests
{
    [Fact]
    public void Create_WithPositiveValue_ReturnsSuccess()
    {
        var result = Quantity.Create(10);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Value);
    }

    [Fact]
    public void Create_WithZero_ReturnsSuccess()
    {
        var result = Quantity.Create(0);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Value);
    }

    [Fact]
    public void Create_WithNegativeValue_ReturnsFailure()
    {
        var result = Quantity.Create(-1);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var q1 = Quantity.Create(5).Value;
        var q2 = Quantity.Create(5).Value;

        Assert.Equal(q1, q2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var q1 = Quantity.Create(5).Value;
        var q2 = Quantity.Create(10).Value;

        Assert.NotEqual(q1, q2);
    }

    [Fact]
    public void Add_TwoQuantities_ReturnsSummedValue()
    {
        var q1 = Quantity.Create(5).Value;
        var q2 = Quantity.Create(3).Value;

        var sum = q1.Add(q2);

        Assert.Equal(8, sum.Value);
    }

    [Fact]
    public void Subtract_ResultNonNegative_ReturnsSuccess()
    {
        var q1 = Quantity.Create(10).Value;
        var q2 = Quantity.Create(3).Value;

        var result = q1.Subtract(q2);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.Value);
    }

    [Fact]
    public void Subtract_ResultNegative_ReturnsFailure()
    {
        var q1 = Quantity.Create(3).Value;
        var q2 = Quantity.Create(5).Value;

        var result = q1.Subtract(q2);

        Assert.True(result.IsFailure);
    }
}
