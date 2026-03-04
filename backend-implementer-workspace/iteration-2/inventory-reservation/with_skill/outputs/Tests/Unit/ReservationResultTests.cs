using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests.Unit;

public class ReservationResultTests
{
    [Fact]
    public void FullyReserved_HasZeroShortfall()
    {
        var reservation = CreateReservation(10);
        var result = ReservationOutcome.FullyReserved(reservation);

        Assert.Equal(0, result.Shortfall);
        Assert.True(result.IsFullyReserved);
    }

    [Fact]
    public void PartiallyReserved_ReportsCorrectShortfall()
    {
        var reservation = CreateReservation(5);
        var result = ReservationOutcome.PartiallyReserved(reservation, 3);

        Assert.Equal(3, result.Shortfall);
        Assert.False(result.IsFullyReserved);
    }

    [Fact]
    public void PartiallyReserved_ReservationHasReservedQuantity()
    {
        var reservation = CreateReservation(5);
        var result = ReservationOutcome.PartiallyReserved(reservation, 3);

        Assert.Equal(5, result.Reservation.Quantity.Value);
    }

    private static Reservation CreateReservation(int qty)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(qty).Value;
        var now = DateTime.UtcNow;
        return Reservation.Create(productId, quantity, now).Value;
    }
}
