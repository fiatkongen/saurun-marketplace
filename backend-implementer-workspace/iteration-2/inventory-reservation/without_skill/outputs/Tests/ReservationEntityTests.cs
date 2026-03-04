using FluentAssertions;
using InventoryReservation.Domain.Aggregates;
using InventoryReservation.Domain.Enums;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests;

public class ReservationEntityTests
{
    private static readonly DateTime BaseTime = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Reservation_IsExpired_ReturnsTrueWhenPastExpiryAndPending()
    {
        var item = new InventoryItem(ProductId.Create(), Quantity.Of(100));
        var result = item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));

        var reservation = item.Reservations[0];
        reservation.IsExpired(BaseTime.AddMinutes(6)).Should().BeTrue();
    }

    [Fact]
    public void Reservation_IsExpired_ReturnsFalseBeforeExpiry()
    {
        var item = new InventoryItem(ProductId.Create(), Quantity.Of(100));
        item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));

        var reservation = item.Reservations[0];
        reservation.IsExpired(BaseTime.AddMinutes(4)).Should().BeFalse();
    }

    [Fact]
    public void Reservation_IsExpired_ReturnsFalseForConfirmedReservation()
    {
        var item = new InventoryItem(ProductId.Create(), Quantity.Of(100));
        var result = item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));
        item.ConfirmReservation(result.Value.ReservationId, BaseTime);

        var reservation = item.Reservations[0];
        // Even though time is past expiry, confirmed reservations are not "expired"
        reservation.IsExpired(BaseTime.AddMinutes(10)).Should().BeFalse();
    }

    [Fact]
    public void Reservation_IsPending_TrueForNewReservation()
    {
        var item = new InventoryItem(ProductId.Create(), Quantity.Of(100));
        item.Reserve(Quantity.Of(10), BaseTime);

        item.Reservations[0].IsPending.Should().BeTrue();
    }

    [Fact]
    public void Reservation_StatusTransitions_AreEnforced()
    {
        // Confirmed reservation cannot be cancelled
        var item = new InventoryItem(ProductId.Create(), Quantity.Of(100));
        var r1 = item.Reserve(Quantity.Of(10), BaseTime);
        item.ConfirmReservation(r1.Value.ReservationId, BaseTime);

        var cancelResult = item.CancelReservation(r1.Value.ReservationId, BaseTime);
        cancelResult.IsFailure.Should().BeTrue();
    }
}
