using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Tests.Unit;

public class ConcurrencyTests
{
    private readonly ProductId _productId = ProductId.Create(Guid.NewGuid()).Value;

    [Fact]
    public void Version_IncreasesWithEachMutation()
    {
        var item = CreateItemWithStock(100);
        Assert.Equal(0, item.Version);

        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        Assert.Equal(1, item.Version);

        item.Reserve(Quantity.Create(5).Value, DateTime.UtcNow.AddMinutes(15));
        Assert.Equal(2, item.Version);
    }

    [Fact]
    public void ConfirmReservation_IncrementsVersion()
    {
        var item = CreateItemWithStock(100);
        var r = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var versionAfterReserve = item.Version;

        item.ConfirmReservation(r.Value.Reservation!.Id);

        Assert.Equal(versionAfterReserve + 1, item.Version);
    }

    [Fact]
    public void CancelReservation_IncrementsVersion()
    {
        var item = CreateItemWithStock(100);
        var r = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var versionAfterReserve = item.Version;

        item.CancelReservation(r.Value.Reservation!.Id);

        Assert.Equal(versionAfterReserve + 1, item.Version);
    }

    [Fact]
    public void ExpireReservation_IncrementsVersion()
    {
        var item = CreateItemWithStock(100);
        var r = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var versionAfterReserve = item.Version;

        item.ExpireReservation(r.Value.Reservation!.Id);

        Assert.Equal(versionAfterReserve + 1, item.Version);
    }

    [Fact]
    public void MultipleReservations_VersionTracksAllMutations()
    {
        var item = CreateItemWithStock(100);

        // 3 reservations
        var r1 = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var r2 = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));
        var r3 = item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        // Confirm 1, cancel 1, expire 1
        item.ConfirmReservation(r1.Value.Reservation!.Id);
        item.CancelReservation(r2.Value.Reservation!.Id);
        item.ExpireReservation(r3.Value.Reservation!.Id);

        Assert.Equal(6, item.Version); // 3 reserves + 3 mutations
    }

    [Fact]
    public void PartialReservation_StillIncrementsVersion()
    {
        var item = CreateItemWithStock(3);

        item.Reserve(Quantity.Create(10).Value, DateTime.UtcNow.AddMinutes(15));

        Assert.Equal(1, item.Version);
    }

    [Fact]
    public void ZeroStockReservation_DoesNotIncrementVersion()
    {
        var item = CreateItemWithStock(0);

        item.Reserve(Quantity.Create(5).Value, DateTime.UtcNow.AddMinutes(15));

        // No actual stock moved, no version increment
        Assert.Equal(0, item.Version);
    }

    private InventoryItem CreateItemWithStock(int stock)
    {
        var available = Quantity.Create(stock).Value;
        return InventoryItem.Create(_productId, available).Value;
    }
}
