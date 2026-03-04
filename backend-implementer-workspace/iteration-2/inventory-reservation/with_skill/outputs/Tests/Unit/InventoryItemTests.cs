using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.Events;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests.Unit;

public class InventoryItemCreateTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(100).Value;

        var result = InventoryItem.Create(productId, quantity);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.AvailableQuantity.Value);
    }

    [Fact]
    public void Create_SetsReservedQuantityToZero()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(50).Value;

        var item = InventoryItem.Create(productId, quantity).Value;

        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void Create_RaisesInventoryItemCreatedEvent()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(50).Value;

        var item = InventoryItem.Create(productId, quantity).Value;

        var evt = Assert.Single(item.DomainEvents);
        var created = Assert.IsType<InventoryItemCreatedEvent>(evt);
        Assert.Equal(50, created.InitialQuantity);
    }

    [Fact]
    public void Create_SetsVersionToZero()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(50).Value;

        var item = InventoryItem.Create(productId, quantity).Value;

        Assert.Equal(0, item.Version);
    }
}

public class InventoryItemReserveTests
{
    private static InventoryItem CreateItem(int available)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(available).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        item.ClearDomainEvents(); // clear creation event for cleaner test assertions
        return item;
    }

    private static DateTime Now() => new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Reserve_WhenFullyAvailable_ReturnsFullReservation()
    {
        var item = CreateItem(100);

        var result = item.Reserve(Quantity.Create(10).Value, Now());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsFullyReserved);
        Assert.Equal(10, result.Value.Reservation.Quantity.Value);
    }

    [Fact]
    public void Reserve_WhenFullyAvailable_DecreasesAvailableAndIncreasesReserved()
    {
        var item = CreateItem(100);

        item.Reserve(Quantity.Create(10).Value, Now());

        Assert.Equal(90, item.AvailableQuantity.Value);
        Assert.Equal(10, item.ReservedQuantity.Value);
    }

    [Fact]
    public void Reserve_WhenPartiallyAvailable_ReservesWhatIsAvailable()
    {
        var item = CreateItem(5);

        var result = item.Reserve(Quantity.Create(10).Value, Now());

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsFullyReserved);
        Assert.Equal(5, result.Value.Shortfall);
        Assert.Equal(5, result.Value.Reservation.Quantity.Value);
    }

    [Fact]
    public void Reserve_WhenNothingAvailable_ReturnsFailure()
    {
        var item = CreateItem(10);
        // Reserve everything first
        item.Reserve(Quantity.Create(10).Value, Now());

        var result = item.Reserve(Quantity.Create(5).Value, Now());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Reserve_RaisesStockReservedEvent()
    {
        var item = CreateItem(100);

        var result = item.Reserve(Quantity.Create(10).Value, Now());

        var evt = Assert.Single(item.DomainEvents);
        var reserved = Assert.IsType<StockReservedEvent>(evt);
        Assert.Equal(10, reserved.Quantity);
        Assert.Equal(0, reserved.Shortfall);
    }

    [Fact]
    public void Reserve_PartialReservation_EventHasCorrectShortfall()
    {
        var item = CreateItem(5);

        item.Reserve(Quantity.Create(8).Value, Now());

        var evt = Assert.Single(item.DomainEvents);
        var reserved = Assert.IsType<StockReservedEvent>(evt);
        Assert.Equal(5, reserved.Quantity);
        Assert.Equal(3, reserved.Shortfall);
    }

    [Fact]
    public void Reserve_IncrementsVersion()
    {
        var item = CreateItem(100);

        item.Reserve(Quantity.Create(10).Value, Now());

        Assert.Equal(1, item.Version);
    }

    [Fact]
    public void Reserve_ExpiresOverdueReservationsFirst()
    {
        var item = CreateItem(10);
        var earlyNow = Now();
        // Reserve all 10
        item.Reserve(Quantity.Create(10).Value, earlyNow);
        item.ClearDomainEvents();

        // 20 minutes later, the first reservation has expired
        var laterNow = earlyNow.AddMinutes(20);
        var result = item.Reserve(Quantity.Create(5).Value, laterNow);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsFullyReserved);
    }

    [Fact]
    public void Reserve_AddsReservationToCollection()
    {
        var item = CreateItem(100);

        item.Reserve(Quantity.Create(10).Value, Now());

        Assert.Single(item.Reservations);
    }

    [Fact]
    public void Reserve_MultipleReservations_AllTracked()
    {
        var item = CreateItem(100);
        var now = Now();

        item.Reserve(Quantity.Create(10).Value, now);
        item.Reserve(Quantity.Create(20).Value, now);

        Assert.Equal(2, item.Reservations.Count);
        Assert.Equal(70, item.AvailableQuantity.Value);
        Assert.Equal(30, item.ReservedQuantity.Value);
    }
}

public class InventoryItemConfirmTests
{
    private static InventoryItem CreateItemWithReservation(int available, int reserveQty, out Guid reservationId)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(available).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var outcome = item.Reserve(Quantity.Create(reserveQty).Value, now).Value;
        reservationId = outcome.Reservation.Id;
        item.ClearDomainEvents();
        return item;
    }

    private static DateTime ConfirmTime() => new(2026, 1, 1, 12, 5, 0, DateTimeKind.Utc);

    [Fact]
    public void ConfirmReservation_WhenPending_DecreasesReservedQuantity()
    {
        var item = CreateItemWithReservation(100, 10, out var reservationId);

        var result = item.ConfirmReservation(reservationId, ConfirmTime());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, item.ReservedQuantity.Value);
        // Available stays at 90 (stock was already committed)
        Assert.Equal(90, item.AvailableQuantity.Value);
    }

    [Fact]
    public void ConfirmReservation_RaisesReservationConfirmedEvent()
    {
        var item = CreateItemWithReservation(100, 10, out var reservationId);

        item.ConfirmReservation(reservationId, ConfirmTime());

        var evt = Assert.Single(item.DomainEvents);
        Assert.IsType<ReservationConfirmedEvent>(evt);
    }

    [Fact]
    public void ConfirmReservation_WithUnknownId_ReturnsFailure()
    {
        var item = CreateItemWithReservation(100, 10, out _);

        var result = item.ConfirmReservation(Guid.NewGuid(), ConfirmTime());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ConfirmReservation_IncrementsVersion()
    {
        var item = CreateItemWithReservation(100, 10, out var reservationId);
        var versionBefore = item.Version;

        item.ConfirmReservation(reservationId, ConfirmTime());

        Assert.Equal(versionBefore + 1, item.Version);
    }
}

public class InventoryItemCancelTests
{
    private static InventoryItem CreateItemWithReservation(int available, int reserveQty, out Guid reservationId)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(available).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var outcome = item.Reserve(Quantity.Create(reserveQty).Value, now).Value;
        reservationId = outcome.Reservation.Id;
        item.ClearDomainEvents();
        return item;
    }

    [Fact]
    public void CancelReservation_WhenPending_ReturnsStockToAvailable()
    {
        var item = CreateItemWithReservation(100, 10, out var reservationId);

        var result = item.CancelReservation(reservationId);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void CancelReservation_RaisesReservationCancelledEvent()
    {
        var item = CreateItemWithReservation(100, 10, out var reservationId);

        item.CancelReservation(reservationId);

        var evt = Assert.Single(item.DomainEvents);
        Assert.IsType<ReservationCancelledEvent>(evt);
    }

    [Fact]
    public void CancelReservation_WithUnknownId_ReturnsFailure()
    {
        var item = CreateItemWithReservation(100, 10, out _);

        var result = item.CancelReservation(Guid.NewGuid());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CancelReservation_IncrementsVersion()
    {
        var item = CreateItemWithReservation(100, 10, out var reservationId);
        var versionBefore = item.Version;

        item.CancelReservation(reservationId);

        Assert.Equal(versionBefore + 1, item.Version);
    }
}

public class InventoryItemExpireTests
{
    [Fact]
    public void ExpireOverdueReservations_ReturnsStockToAvailable()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(100).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        item.Reserve(Quantity.Create(10).Value, now);
        item.ClearDomainEvents();

        var afterExpiry = now.AddMinutes(20);
        var expired = item.ExpireOverdueReservations(afterExpiry);

        Assert.Equal(1, expired);
        Assert.Equal(100, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void ExpireOverdueReservations_DoesNotExpireActiveReservations()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(100).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        item.Reserve(Quantity.Create(10).Value, now);
        item.ClearDomainEvents();

        var beforeExpiry = now.AddMinutes(5);
        var expired = item.ExpireOverdueReservations(beforeExpiry);

        Assert.Equal(0, expired);
        Assert.Equal(90, item.AvailableQuantity.Value);
        Assert.Equal(10, item.ReservedQuantity.Value);
    }

    [Fact]
    public void ExpireOverdueReservations_RaisesEventPerExpiredReservation()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(100).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        item.Reserve(Quantity.Create(10).Value, now);
        item.Reserve(Quantity.Create(20).Value, now);
        item.ClearDomainEvents();

        var afterExpiry = now.AddMinutes(20);
        item.ExpireOverdueReservations(afterExpiry);

        Assert.Equal(2, item.DomainEvents.Count);
        Assert.All(item.DomainEvents, e => Assert.IsType<ReservationExpiredEvent>(e));
    }
}

public class InventoryItemConcurrencyTests
{
    [Fact]
    public void Version_IncreasesOnEachMutatingOperation()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(100).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        Assert.Equal(0, item.Version);

        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        item.Reserve(Quantity.Create(10).Value, now);
        Assert.Equal(1, item.Version);

        var outcome = item.Reserve(Quantity.Create(5).Value, now);
        Assert.Equal(2, item.Version);

        item.ConfirmReservation(outcome.Value.Reservation.Id, now.AddMinutes(1));
        Assert.Equal(3, item.Version);
    }
}
