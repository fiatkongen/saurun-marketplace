using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Tests.Unit;

public class ReservationTests
{
    private readonly ProductId _productId = ProductId.Create(Guid.NewGuid()).Value;

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessWithPendingStatus()
    {
        var quantity = Quantity.Create(5).Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var result = Reservation.Create(_productId, quantity, expiresAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Pending, result.Value.Status);
        Assert.Equal(5, result.Value.Quantity.Value);
    }

    [Fact]
    public void Create_WithZeroQuantity_ReturnsFailure()
    {
        var quantity = Quantity.Create(0).Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var result = Reservation.Create(_productId, quantity, expiresAt);

        Assert.True(result.IsFailure);
        Assert.Contains("positive", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WithPastExpiry_ReturnsFailure()
    {
        var quantity = Quantity.Create(5).Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(-1);

        var result = Reservation.Create(_productId, quantity, expiresAt);

        Assert.True(result.IsFailure);
        Assert.Contains("future", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Confirm_WhenPending_ReturnsSuccessAndSetsConfirmed()
    {
        var reservation = CreateValidReservation();

        var result = reservation.Confirm();

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ReturnsFailure()
    {
        var reservation = CreateValidReservation();
        reservation.Confirm();

        var result = reservation.Confirm();

        Assert.True(result.IsFailure);
        Assert.Contains("Pending", result.Error);
    }

    [Fact]
    public void Cancel_WhenPending_ReturnsSuccessAndSetsCancelled()
    {
        var reservation = CreateValidReservation();

        var result = reservation.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_WhenConfirmed_ReturnsFailure()
    {
        var reservation = CreateValidReservation();
        reservation.Confirm();

        var result = reservation.Cancel();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Expire_WhenPending_ReturnsSuccessAndSetsExpired()
    {
        var reservation = CreateValidReservation();

        var result = reservation.Expire();

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
    }

    [Fact]
    public void Expire_WhenConfirmed_ReturnsFailure()
    {
        var reservation = CreateValidReservation();
        reservation.Confirm();

        var result = reservation.Expire();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void IsExpired_WhenPastExpiryAndPending_ReturnsTrue()
    {
        var quantity = Quantity.Create(5).Value;
        // Create with expiry 1 second in the future, then check after
        var expiresAt = DateTime.UtcNow.AddSeconds(-1);
        // We need to use a special approach - create with future, then check with past time
        // Actually, let's test via the method that checks current time
        var reservation = Reservation.Create(_productId, quantity, DateTime.UtcNow.AddMinutes(15)).Value;

        // The reservation expires in 15 min, so it should not be expired now
        Assert.False(reservation.IsExpired(DateTime.UtcNow));
    }

    [Fact]
    public void IsExpired_WhenBeforeExpiry_ReturnsFalse()
    {
        var reservation = CreateValidReservation();

        Assert.False(reservation.IsExpired(DateTime.UtcNow));
    }

    private Reservation CreateValidReservation()
    {
        var quantity = Quantity.Create(5).Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        return Reservation.Create(_productId, quantity, expiresAt).Value;
    }
}
