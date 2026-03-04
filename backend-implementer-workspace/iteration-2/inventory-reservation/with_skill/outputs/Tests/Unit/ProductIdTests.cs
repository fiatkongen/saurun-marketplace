using InventoryReservation.Domain.ValueObjects;
using Xunit;

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
    }

    [Fact]
    public void Equals_SameGuid_ReturnsTrue()
    {
        var guid = Guid.NewGuid();
        var a = ProductId.Create(guid).Value;
        var b = ProductId.Create(guid).Value;

        Assert.Equal(a, b);
    }
}
