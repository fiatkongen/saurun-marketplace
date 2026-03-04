using FluentAssertions;
using InventoryReservation.Domain.Aggregates;
using InventoryReservation.Domain.Enums;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests;

public class InventoryItemTests
{
    private static readonly DateTime Now = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ProductId DefaultProductId = ProductId.Create();

    private static InventoryItem CreateItem(int stock = 100)
        => new(DefaultProductId, Quantity.Of(stock));

    // ────────────────────────────────────────────
    // Reserve
    // ────────────────────────────────────────────

    [Fact]
    public void Reserve_full_quantity_succeeds()
    {
        var item = CreateItem(10);

        var result = item.Reserve(Quantity.Of(5), Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Should().Be(Quantity.Of(5));
        result.Value.Shortfall.Should().Be(Quantity.Zero);
        result.Value.IsPartial.Should().BeFalse();
        item.AvailableQuantity.Should().Be(Quantity.Of(5));
        item.ReservedQuantity.Should().Be(Quantity.Of(5));
    }

    [Fact]
    public void Reserve_partial_when_insufficient_stock()
    {
        var item = CreateItem(3);

        var result = item.Reserve(Quantity.Of(10), Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Should().Be(Quantity.Of(3));
        result.Value.RequestedQuantity.Should().Be(Quantity.Of(10));
        result.Value.Shortfall.Should().Be(Quantity.Of(7));
        result.Value.IsPartial.Should().BeTrue();
        item.AvailableQuantity.Should().Be(Quantity.Zero);
        item.ReservedQuantity.Should().Be(Quantity.Of(3));
    }

    [Fact]
    public void Reserve_zero_quantity_fails()
    {
        var item = CreateItem(10);

        var result = item.Reserve(Quantity.Zero, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("greater than zero");
    }

    [Fact]
    public void Reserve_when_no_stock_fails()
    {
        var item = CreateItem(0);

        var result = item.Reserve(Quantity.Of(5), Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No stock available");
    }

    [Fact]
    public void Reserve_all_available_stock_then_reserve_again_fails()
    {
        var item = CreateItem(5);
        item.Reserve(Quantity.Of(5), Now);

        var result = item.Reserve(Quantity.Of(1), Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Reserve_creates_reservation_with_15_minute_default_expiry()
    {
        var item = CreateItem(10);

        var result = item.Reserve(Quantity.Of(5), Now);

        var reservation = item.Reservations.Single();
        reservation.ExpiresAt.Should().Be(Now.AddMinutes(15));
        reservation.Status.Should().Be(ReservationStatus.Pending);
    }

    [Fact]
    public void Reserve_with_custom_expiry()
    {
        var item = CreateItem(10);

        item.Reserve(Quantity.Of(5), Now, TimeSpan.FromMinutes(30));

        item.Reservations.Single().ExpiresAt.Should().Be(Now.AddMinutes(30));
    }

    [Fact]
    public void Reserve_increments_version()
    {
        var item = CreateItem(10);
        var initialVersion = item.Version;

        item.Reserve(Quantity.Of(5), Now);

        item.Version.Should().Be(initialVersion + 1);
    }

    [Fact]
    public void Multiple_reservations_tracked_correctly()
    {
        var item = CreateItem(20);

        var r1 = item.Reserve(Quantity.Of(5), Now);
        var r2 = item.Reserve(Quantity.Of(8), Now);

        item.Reservations.Count.Should().Be(2);
        item.AvailableQuantity.Should().Be(Quantity.Of(7));
        item.ReservedQuantity.Should().Be(Quantity.Of(13));
    }

    // ────────────────────────────────────────────
    // Confirm
    // ────────────────────────────────────────────

    [Fact]
    public void Confirm_reservation_permanently_removes_stock()
    {
        var item = CreateItem(10);
        var result = item.Reserve(Quantity.Of(5), Now);
        var reservationId = result.Value.ReservationId;

        var confirmResult = item.ConfirmReservation(reservationId, Now);

        confirmResult.IsSuccess.Should().BeTrue();
        item.AvailableQuantity.Should().Be(Quantity.Of(5)); // unchanged from reserve
        item.ReservedQuantity.Should().Be(Quantity.Zero);   // reserved -> confirmed (consumed)
        item.Reservations.Single().Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_nonexistent_reservation_fails()
    {
        var item = CreateItem(10);

        var result = item.ConfirmReservation(ReservationId.Create(), Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void Confirm_expired_reservation_returns_stock_and_fails()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now, TimeSpan.FromMinutes(5));
        var reservationId = reserveResult.Value.ReservationId;

        var afterExpiry = Now.AddMinutes(10);
        var confirmResult = item.ConfirmReservation(reservationId, afterExpiry);

        confirmResult.IsFailure.Should().BeTrue();
        confirmResult.Error.Should().Contain("expired");
        item.AvailableQuantity.Should().Be(Quantity.Of(10)); // stock returned
        item.ReservedQuantity.Should().Be(Quantity.Zero);
        item.Reservations.Single().Status.Should().Be(ReservationStatus.Expired);
    }

    [Fact]
    public void Confirm_already_confirmed_reservation_fails()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now);
        var reservationId = reserveResult.Value.ReservationId;
        item.ConfirmReservation(reservationId, Now);

        var result = item.ConfirmReservation(reservationId, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Confirm_increments_version()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now);
        var versionAfterReserve = item.Version;

        item.ConfirmReservation(reserveResult.Value.ReservationId, Now);

        item.Version.Should().Be(versionAfterReserve + 1);
    }

    // ────────────────────────────────────────────
    // Cancel
    // ────────────────────────────────────────────

    [Fact]
    public void Cancel_reservation_returns_stock_to_available()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now);
        var reservationId = reserveResult.Value.ReservationId;

        var cancelResult = item.CancelReservation(reservationId);

        cancelResult.IsSuccess.Should().BeTrue();
        item.AvailableQuantity.Should().Be(Quantity.Of(10));
        item.ReservedQuantity.Should().Be(Quantity.Zero);
        item.Reservations.Single().Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_nonexistent_reservation_fails()
    {
        var item = CreateItem(10);

        var result = item.CancelReservation(ReservationId.Create());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_confirmed_reservation_fails()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now);
        var reservationId = reserveResult.Value.ReservationId;
        item.ConfirmReservation(reservationId, Now);

        var cancelResult = item.CancelReservation(reservationId);

        cancelResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_increments_version()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now);
        var versionAfterReserve = item.Version;

        item.CancelReservation(reserveResult.Value.ReservationId);

        item.Version.Should().Be(versionAfterReserve + 1);
    }

    // ────────────────────────────────────────────
    // Expire
    // ────────────────────────────────────────────

    [Fact]
    public void ExpireOverdueReservations_returns_stock()
    {
        var item = CreateItem(10);
        item.Reserve(Quantity.Of(3), Now, TimeSpan.FromMinutes(5));
        item.Reserve(Quantity.Of(4), Now, TimeSpan.FromMinutes(10));

        var afterBothExpired = Now.AddMinutes(15);
        var expiredCount = item.ExpireOverdueReservations(afterBothExpired);

        expiredCount.Should().Be(2);
        item.AvailableQuantity.Should().Be(Quantity.Of(10));
        item.ReservedQuantity.Should().Be(Quantity.Zero);
    }

    [Fact]
    public void ExpireOverdueReservations_only_expires_overdue()
    {
        var item = CreateItem(10);
        item.Reserve(Quantity.Of(3), Now, TimeSpan.FromMinutes(5));
        item.Reserve(Quantity.Of(4), Now, TimeSpan.FromMinutes(30));

        var partialExpiry = Now.AddMinutes(10);
        var expiredCount = item.ExpireOverdueReservations(partialExpiry);

        expiredCount.Should().Be(1);
        item.AvailableQuantity.Should().Be(Quantity.Of(6)); // 3 returned, 4 still reserved
        item.ReservedQuantity.Should().Be(Quantity.Of(4));
    }

    [Fact]
    public void ExpireOverdueReservations_ignores_non_pending()
    {
        var item = CreateItem(10);
        var r1 = item.Reserve(Quantity.Of(3), Now, TimeSpan.FromMinutes(5));
        item.ConfirmReservation(r1.Value.ReservationId, Now);

        var afterExpiry = Now.AddMinutes(10);
        var expiredCount = item.ExpireOverdueReservations(afterExpiry);

        expiredCount.Should().Be(0);
    }

    [Fact]
    public void Reserve_auto_expires_overdue_reservations_first()
    {
        var item = CreateItem(5);
        // Reserve all stock with short expiry
        item.Reserve(Quantity.Of(5), Now, TimeSpan.FromMinutes(5));
        item.AvailableQuantity.Should().Be(Quantity.Zero);

        // Later: stock should be reclaimed and new reservation should succeed
        var afterExpiry = Now.AddMinutes(10);
        var result = item.Reserve(Quantity.Of(3), afterExpiry);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Should().Be(Quantity.Of(3));
        item.AvailableQuantity.Should().Be(Quantity.Of(2));
    }

    // ────────────────────────────────────────────
    // Restock
    // ────────────────────────────────────────────

    [Fact]
    public void Restock_increases_available_quantity()
    {
        var item = CreateItem(5);
        item.Restock(Quantity.Of(10));

        item.AvailableQuantity.Should().Be(Quantity.Of(15));
    }

    [Fact]
    public void Restock_zero_throws()
    {
        var item = CreateItem(5);

        var act = () => item.Restock(Quantity.Zero);
        act.Should().Throw<ArgumentException>();
    }

    // ────────────────────────────────────────────
    // Concurrency (optimistic via version)
    // ────────────────────────────────────────────

    [Fact]
    public void Version_starts_at_zero()
    {
        var item = CreateItem(10);
        item.Version.Should().Be(0);
    }

    [Fact]
    public void Each_mutation_increments_version()
    {
        var item = CreateItem(100);

        item.Reserve(Quantity.Of(10), Now);
        item.Version.Should().Be(1);

        var reservationId = item.Reservations.First().Id;
        item.ConfirmReservation(reservationId, Now);
        item.Version.Should().Be(2);

        var r2 = item.Reserve(Quantity.Of(5), Now);
        item.Version.Should().Be(3);

        item.CancelReservation(r2.Value.ReservationId);
        item.Version.Should().Be(4);

        item.Restock(Quantity.Of(20));
        item.Version.Should().Be(5);
    }

    [Fact]
    public void Simulated_concurrent_reservation_detected_via_version()
    {
        // This simulates what an ORM concurrency check would do:
        // Two "threads" load the same aggregate at the same version,
        // both try to mutate — the second one would fail at persistence
        // because its expected version is stale.
        var item = CreateItem(10);
        var loadedVersion = item.Version; // both "threads" see version 0

        // Thread A reserves
        item.Reserve(Quantity.Of(5), Now);
        item.Version.Should().Be(1);

        // Thread B would check: "I loaded version 0, but current is 1" -> conflict
        var threadBExpectedVersion = loadedVersion;
        var concurrencyConflict = item.Version != threadBExpectedVersion;

        concurrencyConflict.Should().BeTrue("version mismatch indicates concurrent modification");
    }

    [Fact]
    public void Simulated_concurrent_confirm_and_cancel_conflict()
    {
        var item = CreateItem(10);
        var reserveResult = item.Reserve(Quantity.Of(5), Now);
        var reservationId = reserveResult.Value.ReservationId;
        var versionAfterReserve = item.Version;

        // Thread A confirms
        item.ConfirmReservation(reservationId, Now);

        // Thread B tries to cancel — same reservation, but version mismatch
        (item.Version != versionAfterReserve).Should().BeTrue("confirm incremented version");

        // Even without version check, the domain prevents double-mutation
        var cancelResult = item.CancelReservation(reservationId);
        cancelResult.IsFailure.Should().BeTrue("cannot cancel a confirmed reservation");
    }

    // ────────────────────────────────────────────
    // Complex scenarios
    // ────────────────────────────────────────────

    [Fact]
    public void Full_lifecycle_reserve_confirm()
    {
        var item = CreateItem(20);

        // Reserve
        var result = item.Reserve(Quantity.Of(8), Now);
        result.IsSuccess.Should().BeTrue();
        item.AvailableQuantity.Should().Be(Quantity.Of(12));
        item.ReservedQuantity.Should().Be(Quantity.Of(8));

        // Confirm
        item.ConfirmReservation(result.Value.ReservationId, Now);
        item.AvailableQuantity.Should().Be(Quantity.Of(12));
        item.ReservedQuantity.Should().Be(Quantity.Zero);
        // Total effective stock is now 12 (8 consumed)
    }

    [Fact]
    public void Full_lifecycle_reserve_cancel()
    {
        var item = CreateItem(20);

        var result = item.Reserve(Quantity.Of(8), Now);
        item.CancelReservation(result.Value.ReservationId);

        item.AvailableQuantity.Should().Be(Quantity.Of(20));
        item.ReservedQuantity.Should().Be(Quantity.Zero);
    }

    [Fact]
    public void Full_lifecycle_reserve_expire()
    {
        var item = CreateItem(20);

        item.Reserve(Quantity.Of(8), Now, TimeSpan.FromMinutes(10));
        item.ExpireOverdueReservations(Now.AddMinutes(15));

        item.AvailableQuantity.Should().Be(Quantity.Of(20));
        item.ReservedQuantity.Should().Be(Quantity.Zero);
    }

    [Fact]
    public void Mixed_reservations_with_different_outcomes()
    {
        var item = CreateItem(30);

        // Three reservations
        var r1 = item.Reserve(Quantity.Of(10), Now, TimeSpan.FromMinutes(5));
        var r2 = item.Reserve(Quantity.Of(10), Now, TimeSpan.FromMinutes(30));
        var r3 = item.Reserve(Quantity.Of(10), Now, TimeSpan.FromMinutes(30));

        item.AvailableQuantity.Should().Be(Quantity.Zero);
        item.ReservedQuantity.Should().Be(Quantity.Of(30));

        // r1 expires, r2 confirmed, r3 cancelled
        item.ExpireOverdueReservations(Now.AddMinutes(10));
        item.ConfirmReservation(r2.Value.ReservationId, Now.AddMinutes(10));
        item.CancelReservation(r3.Value.ReservationId);

        // r1 (10) returned to available, r2 (10) consumed, r3 (10) returned to available
        item.AvailableQuantity.Should().Be(Quantity.Of(20));
        item.ReservedQuantity.Should().Be(Quantity.Zero);
    }

    [Fact]
    public void Partial_reservation_followed_by_full_reservation_after_restock()
    {
        var item = CreateItem(3);

        // Partial reservation
        var partial = item.Reserve(Quantity.Of(10), Now);
        partial.Value.ReservedQuantity.Should().Be(Quantity.Of(3));
        partial.Value.Shortfall.Should().Be(Quantity.Of(7));

        // Restock
        item.Restock(Quantity.Of(20));
        item.AvailableQuantity.Should().Be(Quantity.Of(20));

        // Now full reservation works
        var full = item.Reserve(Quantity.Of(10), Now);
        full.Value.ReservedQuantity.Should().Be(Quantity.Of(10));
        full.Value.IsPartial.Should().BeFalse();
    }
}
