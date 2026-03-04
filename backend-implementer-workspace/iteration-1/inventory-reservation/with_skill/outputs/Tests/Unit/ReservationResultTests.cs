using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Tests.Unit;

public class ReservationResultTests
{
    [Fact]
    public void IsFull_WhenNoShortfall_ReturnsTrue()
    {
        var reserved = Quantity.Create(10).Value;
        var shortfall = Quantity.Create(0).Value;

        var result = new ReservationResult(reserved, shortfall, null);

        Assert.True(result.IsFull);
    }

    [Fact]
    public void IsFull_WhenShortfall_ReturnsFalse()
    {
        var reserved = Quantity.Create(7).Value;
        var shortfall = Quantity.Create(3).Value;

        var result = new ReservationResult(reserved, shortfall, null);

        Assert.False(result.IsFull);
    }

    [Fact]
    public void IsEmpty_WhenNothingReserved_ReturnsTrue()
    {
        var reserved = Quantity.Create(0).Value;
        var shortfall = Quantity.Create(5).Value;

        var result = new ReservationResult(reserved, shortfall, null);

        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void IsPartial_WhenSomeReservedWithShortfall_ReturnsTrue()
    {
        var reserved = Quantity.Create(3).Value;
        var shortfall = Quantity.Create(7).Value;

        var result = new ReservationResult(reserved, shortfall, null);

        Assert.True(result.IsPartial);
    }
}
