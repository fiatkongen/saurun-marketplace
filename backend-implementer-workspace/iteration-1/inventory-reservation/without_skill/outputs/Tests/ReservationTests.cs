using FluentAssertions;
using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.Enums;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests;

public class ReservationTests
{
    private static readonly DateTime Now = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);

    private static Reservation CreatePendingReservation(TimeSpan? expiresIn = null)
    {
        var expiry = Now.Add(expiresIn ?? TimeSpan.FromMinutes(15));
        return new Reservation(
            ReservationId.Create(),
            ProductId.Create(),
            Quantity.Of(5),
            expiry);
    }

    [Fact]
    public void New_reservation_is_pending()
    {
        var reservation = CreatePendingReservation();
        reservation.Status.Should().Be(ReservationStatus.Pending);
    }

    [Fact]
    public void Confirm_pending_reservation_succeeds()
    {
        var reservation = CreatePendingReservation();
        var result = reservation.Confirm(Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_expired_reservation_fails()
    {
        var reservation = CreatePendingReservation(TimeSpan.FromMinutes(5));
        var afterExpiry = Now.AddMinutes(10);

        var result = reservation.Confirm(afterExpiry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("expired");
    }

    [Fact]
    public void Confirm_already_confirmed_fails()
    {
        var reservation = CreatePendingReservation();
        reservation.Confirm(Now);

        var result = reservation.Confirm(Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_pending_reservation_succeeds()
    {
        var reservation = CreatePendingReservation();
        var result = reservation.Cancel();

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_confirmed_reservation_fails()
    {
        var reservation = CreatePendingReservation();
        reservation.Confirm(Now);

        var result = reservation.Cancel();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Expire_overdue_reservation_succeeds()
    {
        var reservation = CreatePendingReservation(TimeSpan.FromMinutes(5));
        var afterExpiry = Now.AddMinutes(10);

        var result = reservation.Expire(afterExpiry);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Expired);
    }

    [Fact]
    public void Expire_not_yet_overdue_fails()
    {
        var reservation = CreatePendingReservation(TimeSpan.FromMinutes(15));

        var result = reservation.Expire(Now.AddMinutes(5));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_returns_true_when_past_expiry()
    {
        var reservation = CreatePendingReservation(TimeSpan.FromMinutes(5));

        reservation.IsExpired(Now.AddMinutes(10)).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_returns_false_when_not_pending()
    {
        var reservation = CreatePendingReservation(TimeSpan.FromMinutes(5));
        reservation.Confirm(Now);

        // Even though time has passed, confirmed reservations are not "expired"
        reservation.IsExpired(Now.AddMinutes(10)).Should().BeFalse();
    }
}
