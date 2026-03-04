using InventoryReservation.Domain.ValueObjects;

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

    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(0, 10, 10)]
    public void Add_ReturnsSumQuantity(int a, int b, int expected)
    {
        var qa = Quantity.Create(a).Value;
        var qb = Quantity.Create(b).Value;

        var result = qa.Add(qb);

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Subtract_WhenSufficient_ReturnsResult()
    {
        var qa = Quantity.Create(10).Value;
        var qb = Quantity.Create(3).Value;

        var result = qa.Subtract(qb);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.Value);
    }

    [Fact]
    public void Subtract_WhenInsufficient_ReturnsFailure()
    {
        var qa = Quantity.Create(3).Value;
        var qb = Quantity.Create(5).Value;

        var result = qa.Subtract(qb);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var q1 = Quantity.Create(5).Value;
        var q2 = Quantity.Create(5).Value;

        Assert.Equal(q1, q2);
    }
}
