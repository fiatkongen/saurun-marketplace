using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Tests.Unit;

public class ProductIdTests
{
    [Fact]
    public void Create_WithValidGuid_ReturnsSuccess()
    {
        var guid = Guid.NewGuid();
        var result = ProductId.Create(guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Value.Value);
    }

    [Fact]
    public void Create_WithEmptyGuid_ReturnsFailure()
    {
        var result = ProductId.Create(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();
        var p1 = ProductId.Create(guid).Value;
        var p2 = ProductId.Create(guid).Value;

        Assert.Equal(p1, p2);
    }
}
