using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Tests.Unit;

public class InventoryItemTests
{
    private readonly ProductId _productId = ProductId.Create(Guid.NewGuid()).Value;

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var available = Quantity.Create(100).Value;

        var result = InventoryItem.Create(_productId, available);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.AvailableQuantity.Value);
        Assert.Equal(0, result.Value.ReservedQuantity.Value);
    }

    [Fact]
    public void Reserve_WithSufficientStock_ReturnsReservationResult()
    {
        var item = CreateItemWithStock(100);
        var requestedQty = Quantity.Create(10).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Reserved.Value);
        Assert.Equal(0, result.Value.Shortfall.Value);
        Assert.Equal(90, item.AvailableQuantity.Value);
        Assert.Equal(10, item.ReservedQuantity.Value);
    }

    [Fact]
    public void Reserve_WithInsufficientStock_ReturnsPartialReservation()
    {
        var item = CreateItemWithStock(3);
        var requestedQty = Quantity.Create(10).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Reserved.Value);
        Assert.Equal(7, result.Value.Shortfall.Value);
        Assert.Equal(0, item.AvailableQuantity.Value);
        Assert.Equal(3, item.ReservedQuantity.Value);
    }

    [Fact]
    public void Reserve_WithZeroStock_ReturnsPartialWithFullShortfall()
    {
        var item = CreateItemWithStock(0);
        var requestedQty = Quantity.Create(5).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.Reserved.Value);
        Assert.Equal(5, result.Value.Shortfall.Value);
    }

    [Fact]
    public void Reserve_CreatesReservationEntity()
    {
        var item = CreateItemWithStock(100);
        var requestedQty = Quantity.Create(10).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Reservation);
        Assert.Equal(ReservationStatus.Pending, result.Value.Reservation!.Status);
    }

    [Fact]
    public void Reserve_WithZeroAvailable_ReturnsNullReservation()
    {
        var item = CreateItemWithStock(0);
        var requestedQty = Quantity.Create(5).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Reservation);
    }

    [Fact]
    public void Reserve_WithZeroQuantity_ReturnsFailure()
    {
        var item = CreateItemWithStock(100);
        var requestedQty = Quantity.Create(0).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ConfirmReservation_WithValidReservation_DecrementsReserved()
    {
        var item = CreateItemWithStock(100);
        var reserveResult = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var reservationId = reserveResult.Value.Reservation!.Id;

        var result = item.ConfirmReservation(reservationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(90, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void ConfirmReservation_WithInvalidId_ReturnsFailure()
    {
        var item = CreateItemWithStock(100);

        var result = item.ConfirmReservation(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CancelReservation_WithValidReservation_ReturnsStockToAvailable()
    {
        var item = CreateItemWithStock(100);
        var reserveResult = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var reservationId = reserveResult.Value.Reservation!.Id;

        var result = item.CancelReservation(reservationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void CancelReservation_WithInvalidId_ReturnsFailure()
    {
        var item = CreateItemWithStock(100);

        var result = item.CancelReservation(Guid.NewGuid());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ExpireReservation_ReturnsStockToAvailable()
    {
        var item = CreateItemWithStock(100);
        var reserveResult = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var reservationId = reserveResult.Value.Reservation!.Id;

        var result = item.ExpireReservation(reservationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void MultipleReservations_TracksCorrectly()
    {
        var item = CreateItemWithStock(100);

        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        item.Reserve(Quantity.Create(20).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.Equal(70, item.AvailableQuantity.Value);
        Assert.Equal(30, item.ReservedQuantity.Value);
    }

    [Fact]
    public void ConfirmThenReserve_MaintainsCorrectStock()
    {
        var item = CreateItemWithStock(100);
        var r1 = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        item.ConfirmReservation(r1.Value.Reservation!.Id);

        // After confirm: available=90, reserved=0
        var r2 = item.Reserve(Quantity.Create(30).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.Equal(60, item.AvailableQuantity.Value);
        Assert.Equal(30, item.ReservedQuantity.Value);
    }

    [Fact]
    public void CancelThenReserve_MaintainsCorrectStock()
    {
        var item = CreateItemWithStock(20);
        var r1 = item.Reserve(Quantity.Create(15).Value, DateTime.UtcNow.AddMinutes(15));
        item.CancelReservation(r1.Value.Reservation!.Id);

        // After cancel: available=20, reserved=0
        var r2 = item.Reserve(Quantity.Create(20).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.True(r2.IsSuccess);
        Assert.Equal(20, r2.Value.Reserved.Value);
        Assert.Equal(0, r2.Value.Shortfall.Value);
    }

    [Fact]
    public void Reserve_RaisesStockReservedEvent()
    {
        var item = CreateItemWithStock(100);

        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.Contains(item.DomainEvents, e => e is Domain.Events.StockReservedEvent);
    }

    [Fact]
    public void ConfirmReservation_RaisesReservationConfirmedEvent()
    {
        var item = CreateItemWithStock(100);
        var r = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        item.ConfirmReservation(r.Value.Reservation!.Id);

        Assert.Contains(item.DomainEvents, e => e is Domain.Events.ReservationConfirmedEvent);
    }

    [Fact]
    public void CancelReservation_RaisesReservationCancelledEvent()
    {
        var item = CreateItemWithStock(100);
        var r = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        item.CancelReservation(r.Value.Reservation!.Id);

        Assert.Contains(item.DomainEvents, e => e is Domain.Events.ReservationCancelledEvent);
    }

    [Fact]
    public void Create_RaisesInventoryItemCreatedEvent()
    {
        var available = Quantity.Create(100).Value;

        var result = InventoryItem.Create(_productId, available);

        Assert.Contains(result.Value.DomainEvents, e => e is Domain.Events.InventoryItemCreatedEvent);
    }

    [Fact]
    public void Version_InitializesAtZero()
    {
        var item = CreateItemWithStock(100);

        Assert.Equal(0, item.Version);
    }

    [Fact]
    public void Reserve_IncrementsVersion()
    {
        var item = CreateItemWithStock(100);

        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.Equal(1, item.Version);
    }

    [Fact]
    public void PartialReservation_ReturnsCorrectShortfall()
    {
        var item = CreateItemWithStock(7);
        var requestedQty = Quantity.Create(10).Value;

        var result = item.Reserve(requestedQty, DateTime.UtcNow.AddMinutes(15));

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value.Reserved.Value);
        Assert.Equal(3, result.Value.Shortfall.Value);
        Assert.NotNull(result.Value.Reservation);
    }

    [Fact]
    public void Reservations_AreReadOnlyCollection()
    {
        var item = CreateItemWithStock(100);
        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.Single(item.Reservations);
    }

    [Fact]
    public void ExpireReservations_ExpiresOnlyPastDueReservations()
    {
        var item = CreateItemWithStock(100);
        // Reserve with short expiry
        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddSeconds(1));
        // Reserve with long expiry
        item.Reserve(Quantity.Create(20).Value, DateTime.UtcNow.AddMinutes(30));

        // Expire at a time after the first expires but before the second
        var result = item.ExpireOverdueReservations(DateTime.UtcNow.AddMinutes(1));

        Assert.Equal(1, result);
        // 100 - 10 - 20 = 70 available after both reservations
        // + 10 returned from expired reservation = 80 available
        Assert.Equal(80, item.AvailableQuantity.Value);
        Assert.Equal(20, item.ReservedQuantity.Value);
    }

    private InventoryItem CreateItemWithStock(int stock)
    {
        var available = Quantity.Create(stock).Value;
        return InventoryItem.Create(_productId, available).Value;
    }
}
