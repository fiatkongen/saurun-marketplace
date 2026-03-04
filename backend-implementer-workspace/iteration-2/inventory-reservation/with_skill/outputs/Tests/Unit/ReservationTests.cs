using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests.Unit;

public class ReservationTests
{
    private static ProductId TestProductId() =>
        ProductId.Create(Guid.NewGuid()).Value;

    private static Quantity Qty(int v) => Quantity.Create(v).Value;

    [Fact]
    public void Create_WithValidInputs_ReturnsPendingReservation()
    {
        var productId = TestProductId();
        var quantity = Qty(5);
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = Reservation.Create(productId, quantity, now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Pending, result.Value.Status);
        Assert.Equal(now.AddMinutes(15), result.Value.ExpiresAt);
    }

    [Fact]
    public void Confirm_WhenPending_SetsStatusToConfirmed()
    {
        var reservation = CreatePendingReservation();
        var now = reservation.ExpiresAt.AddMinutes(-5);

        var result = reservation.Confirm(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ReturnsFailure()
    {
        var reservation = CreatePendingReservation();
        var now = reservation.ExpiresAt.AddMinutes(-5);
        reservation.Confirm(now);

        var result = reservation.Confirm(now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Confirm_WhenExpired_ReturnsFailure()
    {
        var reservation = CreatePendingReservation();
        var afterExpiry = reservation.ExpiresAt.AddMinutes(1);

        var result = reservation.Confirm(afterExpiry);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Cancel_WhenPending_SetsStatusToCancelled()
    {
        var reservation = CreatePendingReservation();

        var result = reservation.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_WhenConfirmed_ReturnsFailure()
    {
        var reservation = CreatePendingReservation();
        reservation.Confirm(reservation.ExpiresAt.AddMinutes(-1));

        var result = reservation.Cancel();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Expire_WhenPendingAndPastExpiry_SetsStatusToExpired()
    {
        var reservation = CreatePendingReservation();
        var afterExpiry = reservation.ExpiresAt.AddSeconds(1);

        var result = reservation.Expire(afterExpiry);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
    }

    [Fact]
    public void Expire_WhenPendingButNotYetExpired_ReturnsFailure()
    {
        var reservation = CreatePendingReservation();
        var beforeExpiry = reservation.ExpiresAt.AddMinutes(-1);

        var result = reservation.Expire(beforeExpiry);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void IsExpired_WhenPastExpiryAndPending_ReturnsTrue()
    {
        var reservation = CreatePendingReservation();
        var afterExpiry = reservation.ExpiresAt.AddMinutes(1);

        Assert.True(reservation.IsExpired(afterExpiry));
    }

    [Fact]
    public void IsExpired_WhenBeforeExpiry_ReturnsFalse()
    {
        var reservation = CreatePendingReservation();
        var beforeExpiry = reservation.ExpiresAt.AddMinutes(-1);

        Assert.False(reservation.IsExpired(beforeExpiry));
    }

    private Reservation CreatePendingReservation()
    {
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        return Reservation.Create(TestProductId(), Qty(5), now).Value;
    }
}
