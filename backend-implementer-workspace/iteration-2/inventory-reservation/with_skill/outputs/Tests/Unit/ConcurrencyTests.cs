using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests.Unit;

/// <summary>
/// Tests that demonstrate how optimistic concurrency would work.
/// The Version field is incremented on every mutation, and in a real
/// EF Core scenario, DbUpdateConcurrencyException would be thrown
/// if two transactions read the same version and both try to save.
/// </summary>
public class ConcurrencyTests
{
    private static InventoryItem CreateItem(int available)
    {
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var quantity = Quantity.Create(available).Value;
        return InventoryItem.Create(productId, quantity).Value;
    }

    private static DateTime Now() => new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void SimulatedConcurrentReservations_VersionDetectsConflict()
    {
        // Two "readers" read the same item at version 0
        var item = CreateItem(10);
        var versionAtRead = item.Version; // 0

        // First writer reserves successfully
        item.Reserve(Quantity.Create(8).Value, Now());
        Assert.Equal(versionAtRead + 1, item.Version); // version is now 1

        // A second writer that read version 0 would fail optimistic concurrency check
        // because item.Version (1) != versionAtRead (0)
        Assert.NotEqual(versionAtRead, item.Version);
    }

    [Fact]
    public void SimulatedConcurrentReserveAndConfirm_VersionConflicts()
    {
        var item = CreateItem(50);
        var now = Now();
        var outcome = item.Reserve(Quantity.Create(10).Value, now).Value;
        var versionAfterReserve = item.Version; // 1

        // Confirm changes version
        item.ConfirmReservation(outcome.Reservation.Id, now.AddMinutes(1));

        // Version changed, stale reader would detect conflict
        Assert.NotEqual(versionAfterReserve, item.Version);
        Assert.Equal(versionAfterReserve + 1, item.Version);
    }

    [Fact]
    public void SimulatedRaceCondition_LastOneStandingGetsLessThanExpected()
    {
        // Demonstrates why optimistic concurrency is needed:
        // Two concurrent reservation attempts on low stock
        var item = CreateItem(5);
        var now = Now();

        // First reservation takes all 5
        var result1 = item.Reserve(Quantity.Create(5).Value, now);
        Assert.True(result1.IsSuccess);
        Assert.True(result1.Value.IsFullyReserved);

        // Second reservation finds nothing available
        var result2 = item.Reserve(Quantity.Create(5).Value, now);
        Assert.True(result2.IsFailure);
    }

    [Fact]
    public void SequentialOperations_VersionIsFullyPredictable()
    {
        var item = CreateItem(100);
        var now = Now();
        Assert.Equal(0, item.Version);

        // Each operation increments version by exactly 1
        var o1 = item.Reserve(Quantity.Create(10).Value, now).Value;
        Assert.Equal(1, item.Version);

        var o2 = item.Reserve(Quantity.Create(10).Value, now).Value;
        Assert.Equal(2, item.Version);

        item.ConfirmReservation(o1.Reservation.Id, now.AddMinutes(1));
        Assert.Equal(3, item.Version);

        item.CancelReservation(o2.Reservation.Id);
        Assert.Equal(4, item.Version);
    }
}
