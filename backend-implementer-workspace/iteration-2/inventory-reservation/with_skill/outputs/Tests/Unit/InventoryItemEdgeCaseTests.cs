using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.Events;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests.Unit;

public class InventoryItemEdgeCaseTests
{
    private static InventoryItem CreateItem(int available)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(available).Value;
        var item = InventoryItem.Create(productId, quantity).Value;
        item.ClearDomainEvents();
        return item;
    }

    private static DateTime Now() => new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Reserve_ThenCancel_ThenReserveAgain_StockIsConsistent()
    {
        var item = CreateItem(10);
        var now = Now();

        // Reserve 10, cancel, reserve 10 again
        var outcome1 = item.Reserve(Quantity.Create(10).Value, now).Value;
        item.CancelReservation(outcome1.Reservation.Id);
        var outcome2 = item.Reserve(Quantity.Create(10).Value, now);

        Assert.True(outcome2.IsSuccess);
        Assert.True(outcome2.Value.IsFullyReserved);
        Assert.Equal(0, item.AvailableQuantity.Value);
        Assert.Equal(10, item.ReservedQuantity.Value);
    }

    [Fact]
    public void Reserve_ThenConfirm_AvailableAndReservedAreCorrect()
    {
        var item = CreateItem(50);
        var now = Now();

        var outcome = item.Reserve(Quantity.Create(20).Value, now).Value;
        item.ConfirmReservation(outcome.Reservation.Id, now.AddMinutes(1));

        // Available was 50, reserved 20 -> available=30, reserved=20
        // After confirm: reserved goes to 0, available stays 30 (stock is consumed)
        Assert.Equal(30, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void ConfirmReservation_AfterExpiry_ReturnsFailure()
    {
        var item = CreateItem(50);
        var now = Now();
        var outcome = item.Reserve(Quantity.Create(10).Value, now).Value;
        item.ClearDomainEvents();

        var afterExpiry = now.AddMinutes(20);
        var result = item.ConfirmReservation(outcome.Reservation.Id, afterExpiry);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CancelReservation_AlreadyConfirmed_ReturnsFailure()
    {
        var item = CreateItem(50);
        var now = Now();
        var outcome = item.Reserve(Quantity.Create(10).Value, now).Value;
        item.ConfirmReservation(outcome.Reservation.Id, now.AddMinutes(1));

        var result = item.CancelReservation(outcome.Reservation.Id);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MultiplePartialReservations_DrainStock_ThenExpire_Refill()
    {
        var item = CreateItem(15);
        var now = Now();

        // Reserve 10 (leaves 5 available)
        item.Reserve(Quantity.Create(10).Value, now);
        // Partial reserve 8 (only 5 available, shortfall 3)
        var partial = item.Reserve(Quantity.Create(8).Value, now).Value;

        Assert.Equal(5, partial.Reservation.Quantity.Value);
        Assert.Equal(3, partial.Shortfall);
        Assert.Equal(0, item.AvailableQuantity.Value);
        Assert.Equal(15, item.ReservedQuantity.Value);

        // Expire both (20 min later)
        var afterExpiry = now.AddMinutes(20);
        var expired = item.ExpireOverdueReservations(afterExpiry);

        Assert.Equal(2, expired);
        Assert.Equal(15, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }

    [Fact]
    public void Reserve_ExactlyAvailable_IsFullyReserved()
    {
        var item = CreateItem(10);

        var result = item.Reserve(Quantity.Create(10).Value, Now());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsFullyReserved);
        Assert.Equal(0, item.AvailableQuantity.Value);
    }

    [Fact]
    public void Reserve_OneMoreThanAvailable_IsPartial()
    {
        var item = CreateItem(10);

        var result = item.Reserve(Quantity.Create(11).Value, Now());

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsFullyReserved);
        Assert.Equal(1, result.Value.Shortfall);
        Assert.Equal(10, result.Value.Reservation.Quantity.Value);
    }

    [Fact]
    public void ExpireOverdueReservations_MixedStatusReservations_OnlyExpiresPending()
    {
        var item = CreateItem(100);
        var now = Now();

        // Create two reservations
        var outcome1 = item.Reserve(Quantity.Create(10).Value, now).Value;
        var outcome2 = item.Reserve(Quantity.Create(20).Value, now).Value;

        // Confirm first one
        item.ConfirmReservation(outcome1.Reservation.Id, now.AddMinutes(1));
        item.ClearDomainEvents();

        // Try to expire after 20 min -- only second should expire
        var afterExpiry = now.AddMinutes(20);
        var expired = item.ExpireOverdueReservations(afterExpiry);

        Assert.Equal(1, expired);
        // Available: started 100, reserved 10+20=30 -> available=70
        // Confirmed 10 -> reserved=20, available=70
        // Expired 20 -> reserved=0, available=90
        Assert.Equal(90, item.AvailableQuantity.Value);
        Assert.Equal(0, item.ReservedQuantity.Value);
    }
}

public class InventoryItemVersionTests
{
    [Fact]
    public void Version_StartsAtZero()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = InventoryItem.Create(productId, Quantity.Create(10).Value).Value;

        Assert.Equal(0, item.Version);
    }

    [Fact]
    public void Version_MonotonicallyIncreases_AcrossOperations()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = InventoryItem.Create(productId, Quantity.Create(100).Value).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var versions = new List<int> { item.Version }; // 0

        item.Reserve(Quantity.Create(10).Value, now);
        versions.Add(item.Version); // 1

        var outcome = item.Reserve(Quantity.Create(5).Value, now);
        versions.Add(item.Version); // 2

        item.CancelReservation(outcome.Value.Reservation.Id);
        versions.Add(item.Version); // 3

        // Verify monotonic increase
        for (int i = 1; i < versions.Count; i++)
        {
            Assert.True(versions[i] > versions[i - 1]);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Reserve_NTimes_VersionEqualsN(int count)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = InventoryItem.Create(productId, Quantity.Create(1000).Value).Value;
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < count; i++)
            item.Reserve(Quantity.Create(1).Value, now);

        Assert.Equal(count, item.Version);
    }
}

public class InventoryItemDomainEventTests
{
    [Fact]
    public void Reserve_FullReservation_EventHasZeroShortfall()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = InventoryItem.Create(productId, Quantity.Create(100).Value).Value;
        item.ClearDomainEvents();

        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow);

        var evt = Assert.IsType<StockReservedEvent>(Assert.Single(item.DomainEvents));
        Assert.Equal(0, evt.Shortfall);
        Assert.Equal(10, evt.Quantity);
    }

    [Fact]
    public void Reserve_PartialReservation_EventReportsShortfall()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = InventoryItem.Create(productId, Quantity.Create(5).Value).Value;
        item.ClearDomainEvents();

        item.Reserve(Quantity.Create(8).Value, DateTime.UtcNow);

        var evt = Assert.IsType<StockReservedEvent>(Assert.Single(item.DomainEvents));
        Assert.Equal(3, evt.Shortfall);
        Assert.Equal(5, evt.Quantity);
    }

    [Fact]
    public void CancelReservation_EventContainsQuantity()
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = InventoryItem.Create(productId, Quantity.Create(100).Value).Value;
        var now = DateTime.UtcNow;
        var outcome = item.Reserve(Quantity.Create(15).Value, now).Value;
        item.ClearDomainEvents();

        item.CancelReservation(outcome.Reservation.Id);

        var evt = Assert.IsType<ReservationCancelledEvent>(Assert.Single(item.DomainEvents));
        Assert.Equal(15, evt.Quantity);
    }
}
