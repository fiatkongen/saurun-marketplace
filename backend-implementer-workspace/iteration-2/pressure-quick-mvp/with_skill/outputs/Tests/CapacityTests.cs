using Domain.ValueObjects;

namespace Tests;

public class CapacityTests
{
    [Fact]
    public void Create_WithPositiveValue_ReturnsSuccess()
    {
        var result = Capacity.Create(10);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.MaxCapacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeValue_ReturnsFailure(int value)
    {
        var result = Capacity.Create(value);

        Assert.True(result.IsFailure);
        Assert.Contains("capacity", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var cap1 = Capacity.Create(5).Value;
        var cap2 = Capacity.Create(5).Value;

        Assert.Equal(cap1, cap2);
    }
}
