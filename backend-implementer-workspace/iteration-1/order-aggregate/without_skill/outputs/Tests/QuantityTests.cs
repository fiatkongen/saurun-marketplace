using Domain;
using Xunit;

namespace Tests;

public class QuantityTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsSuccess()
    {
        var result = Quantity.Create(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Value);
    }

    [Fact]
    public void Create_WithOne_ReturnsSuccess()
    {
        var result = Quantity.Create(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Value);
    }

    [Fact]
    public void Create_WithZero_ReturnsFailure()
    {
        var result = Quantity.Create(0);

        Assert.True(result.IsFailure);
        Assert.Contains("at least 1", result.Error!);
    }

    [Fact]
    public void Create_WithNegativeValue_ReturnsFailure()
    {
        var result = Quantity.Create(-3);

        Assert.True(result.IsFailure);
        Assert.Contains("at least 1", result.Error!);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Quantity.Create(5).Value!;
        var b = Quantity.Create(5).Value!;

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        var a = Quantity.Create(5).Value!;
        var b = Quantity.Create(10).Value!;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        var qty = Quantity.Create(42).Value!;

        Assert.Equal("42", qty.ToString());
    }
}
