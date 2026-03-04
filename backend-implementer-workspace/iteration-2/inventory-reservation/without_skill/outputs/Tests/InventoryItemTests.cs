using FluentAssertions;
using InventoryReservation.Domain.Aggregates;
using InventoryReservation.Domain.Enums;
using InventoryReservation.Domain.ValueObjects;
using Xunit;

namespace InventoryReservation.Tests;

public class InventoryItemTests
{
    private static readonly DateTime BaseTime = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    private static InventoryItem CreateItem(int stock = 100) =>
        new(ProductId.Create(), Quantity.Of(stock));

    // =====================
    // RESERVE — Happy Path
    // =====================

    [Fact]
    public void Reserve_WithSufficientStock_Succeeds()
    {
        var item = CreateItem(100);

        var result = item.Reserve(Quantity.Of(10), BaseTime);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Value.Should().Be(10);
        result.Value.Shortfall.Value.Should().Be(0);
        result.Value.IsPartial.Should().BeFalse();
        item.AvailableQuantity.Value.Should().Be(90);
        item.ReservedQuantity.Value.Should().Be(10);
    }

    [Fact]
    public void Reserve_CreatesReservation_WithPendingStatus()
    {
        var item = CreateItem(50);

        var result = item.Reserve(Quantity.Of(5), BaseTime);

        item.Reservations.Should().HaveCount(1);
        var reservation = item.Reservations[0];
        reservation.Id.Should().Be(result.Value.ReservationId);
        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.Quantity.Value.Should().Be(5);
        reservation.ExpiresAt.Should().Be(BaseTime.Add(DefaultExpiration));
    }

    [Fact]
    public void Reserve_VersionIncrementsOnEachOperation()
    {
        var item = CreateItem(100);
        item.Version.Should().Be(0);

        item.Reserve(Quantity.Of(10), BaseTime);
        item.Version.Should().Be(1);

        item.Reserve(Quantity.Of(10), BaseTime);
        item.Version.Should().Be(2);
    }

    [Fact]
    public void Reserve_MultipleReservations_TracksCumulativeQuantity()
    {
        var item = CreateItem(100);

        item.Reserve(Quantity.Of(30), BaseTime);
        item.Reserve(Quantity.Of(20), BaseTime);

        item.AvailableQuantity.Value.Should().Be(50);
        item.ReservedQuantity.Value.Should().Be(50);
        item.Reservations.Should().HaveCount(2);
    }

    // =====================
    // RESERVE — Partial Fill
    // =====================

    [Fact]
    public void Reserve_PartialFill_ReservesWhatIsAvailable()
    {
        var item = CreateItem(7);

        var result = item.Reserve(Quantity.Of(20), BaseTime);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Value.Should().Be(7);
        result.Value.Shortfall.Value.Should().Be(13);
        result.Value.IsPartial.Should().BeTrue();
        item.AvailableQuantity.Value.Should().Be(0);
        item.ReservedQuantity.Value.Should().Be(7);
    }

    [Fact]
    public void Reserve_ExactlyAvailableQuantity_FullFill()
    {
        var item = CreateItem(25);

        var result = item.Reserve(Quantity.Of(25), BaseTime);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Value.Should().Be(25);
        result.Value.Shortfall.Value.Should().Be(0);
        result.Value.IsPartial.Should().BeFalse();
    }

    // =====================
    // RESERVE — Edge Cases
    // =====================

    [Fact]
    public void Reserve_ZeroQuantity_Fails()
    {
        var item = CreateItem(100);

        var result = item.Reserve(Quantity.Of(0), BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("greater than zero");
    }

    [Fact]
    public void Reserve_NoStockAvailable_Fails()
    {
        var item = CreateItem(0);

        var result = item.Reserve(Quantity.Of(5), BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No stock available");
    }

    [Fact]
    public void Reserve_AllStockAlreadyReserved_Fails()
    {
        var item = CreateItem(10);
        item.Reserve(Quantity.Of(10), BaseTime); // reserve all

        var result = item.Reserve(Quantity.Of(1), BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No stock available");
    }

    [Fact]
    public void Reserve_CustomExpiration_SetsCorrectExpiry()
    {
        var item = CreateItem(100);
        var customExpiry = TimeSpan.FromMinutes(30);

        item.Reserve(Quantity.Of(5), BaseTime, customExpiry);

        item.Reservations[0].ExpiresAt.Should().Be(BaseTime.Add(customExpiry));
    }

    // =====================
    // CONFIRM — Happy Path
    // =====================

    [Fact]
    public void Confirm_PendingReservation_Succeeds()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);
        var reservationId = reserveResult.Value.ReservationId;

        var confirmResult = item.ConfirmReservation(reservationId, BaseTime);

        confirmResult.IsSuccess.Should().BeTrue();
        var reservation = item.Reservations[0];
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_DecreasesReservedQuantity_DoesNotChangeAvailable()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);

        // After reserve: available=90, reserved=10
        item.ConfirmReservation(reserveResult.Value.ReservationId, BaseTime);

        // After confirm: available=90 (unchanged), reserved=0 (stock is "sold")
        item.AvailableQuantity.Value.Should().Be(90);
        item.ReservedQuantity.Value.Should().Be(0);
    }

    // =====================
    // CONFIRM — Edge Cases
    // =====================

    [Fact]
    public void Confirm_NonExistentReservation_Fails()
    {
        var item = CreateItem(100);

        var result = item.ConfirmReservation(ReservationId.Create(), BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void Confirm_ExpiredReservation_FailsAndExpiresIt()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);
        var reservationId = reserveResult.Value.ReservationId;

        // Time passes beyond expiration
        var expiredTime = BaseTime.AddMinutes(16);
        var result = item.ConfirmReservation(reservationId, expiredTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("expired");
        item.Reservations[0].Status.Should().Be(ReservationStatus.Expired);
        // Stock should be returned
        item.AvailableQuantity.Value.Should().Be(100);
        item.ReservedQuantity.Value.Should().Be(0);
    }

    [Fact]
    public void Confirm_AlreadyConfirmedReservation_Fails()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);
        var reservationId = reserveResult.Value.ReservationId;
        item.ConfirmReservation(reservationId, BaseTime);

        var result = item.ConfirmReservation(reservationId, BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Confirmed");
    }

    [Fact]
    public void Confirm_CancelledReservation_Fails()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);
        var reservationId = reserveResult.Value.ReservationId;
        item.CancelReservation(reservationId, BaseTime);

        var result = item.ConfirmReservation(reservationId, BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cancelled");
    }

    // =====================
    // CANCEL — Happy Path
    // =====================

    [Fact]
    public void Cancel_PendingReservation_ReturnsStockToAvailable()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);

        var cancelResult = item.CancelReservation(
            reserveResult.Value.ReservationId, BaseTime);

        cancelResult.IsSuccess.Should().BeTrue();
        item.AvailableQuantity.Value.Should().Be(100);
        item.ReservedQuantity.Value.Should().Be(0);
        item.Reservations[0].Status.Should().Be(ReservationStatus.Cancelled);
    }

    // =====================
    // CANCEL — Edge Cases
    // =====================

    [Fact]
    public void Cancel_NonExistentReservation_Fails()
    {
        var item = CreateItem(100);

        var result = item.CancelReservation(ReservationId.Create(), BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void Cancel_ExpiredReservation_FailsWithExpiredMessage()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);

        var expiredTime = BaseTime.AddMinutes(16);
        var result = item.CancelReservation(
            reserveResult.Value.ReservationId, expiredTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("expired");
    }

    [Fact]
    public void Cancel_AlreadyConfirmedReservation_Fails()
    {
        var item = CreateItem(100);
        var reserveResult = item.Reserve(Quantity.Of(10), BaseTime);
        item.ConfirmReservation(reserveResult.Value.ReservationId, BaseTime);

        var result = item.CancelReservation(
            reserveResult.Value.ReservationId, BaseTime);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Confirmed");
    }

    // =====================
    // EXPIRATION
    // =====================

    [Fact]
    public void ExpireStaleReservations_ExpiresOnlyPendingPastDeadline()
    {
        var item = CreateItem(100);

        // Reserve two items at different times
        var r1 = item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));
        var r2 = item.Reserve(Quantity.Of(20), BaseTime, TimeSpan.FromMinutes(30));

        // 10 minutes later: r1 should be expired, r2 still pending
        var checkTime = BaseTime.AddMinutes(10);
        var expiredCount = item.ExpireStaleReservations(checkTime);

        expiredCount.Should().Be(1);
        item.Reservations[0].Status.Should().Be(ReservationStatus.Expired);
        item.Reservations[1].Status.Should().Be(ReservationStatus.Pending);
        item.AvailableQuantity.Value.Should().Be(80); // 10 returned, 20 still reserved
        item.ReservedQuantity.Value.Should().Be(20);
    }

    [Fact]
    public void ExpireStaleReservations_DoesNotExpireConfirmedOrCancelled()
    {
        var item = CreateItem(100);

        var r1 = item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));
        var r2 = item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));
        var r3 = item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));

        item.ConfirmReservation(r1.Value.ReservationId, BaseTime);
        item.CancelReservation(r2.Value.ReservationId, BaseTime);

        // r1=Confirmed, r2=Cancelled, r3=Pending (all past expiry)
        var checkTime = BaseTime.AddMinutes(10);
        var expiredCount = item.ExpireStaleReservations(checkTime);

        expiredCount.Should().Be(1); // Only r3
        item.Reservations[0].Status.Should().Be(ReservationStatus.Confirmed);
        item.Reservations[1].Status.Should().Be(ReservationStatus.Cancelled);
        item.Reservations[2].Status.Should().Be(ReservationStatus.Expired);
    }

    [Fact]
    public void Reserve_AutoExpiresStaleReservations_BeforeProcessing()
    {
        var item = CreateItem(10);
        item.Reserve(Quantity.Of(10), BaseTime, TimeSpan.FromMinutes(5));

        // All stock reserved. But 10 minutes later, the old reservation should auto-expire.
        var laterTime = BaseTime.AddMinutes(10);
        var result = item.Reserve(Quantity.Of(5), laterTime);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservedQuantity.Value.Should().Be(5);
        item.AvailableQuantity.Value.Should().Be(5);
        item.ReservedQuantity.Value.Should().Be(5);
    }

    [Fact]
    public void ExpiresAt_DefaultsTo15Minutes()
    {
        var item = CreateItem(100);
        item.Reserve(Quantity.Of(10), BaseTime);

        item.Reservations[0].ExpiresAt.Should().Be(
            BaseTime.AddMinutes(15));
    }

    // =====================
    // RESTOCK
    // =====================

    [Fact]
    public void Restock_IncreasesAvailableQuantity()
    {
        var item = CreateItem(50);

        item.Restock(Quantity.Of(25));

        item.AvailableQuantity.Value.Should().Be(75);
    }

    [Fact]
    public void Restock_ZeroQuantity_Throws()
    {
        var item = CreateItem(50);

        var act = () => item.Restock(Quantity.Of(0));

        act.Should().Throw<ArgumentException>();
    }

    // =====================
    // CONCURRENCY (Version tracking)
    // =====================

    [Fact]
    public void Version_IncreasesOnEveryMutatingOperation()
    {
        var item = CreateItem(100);
        item.Version.Should().Be(0);

        var r1 = item.Reserve(Quantity.Of(10), BaseTime);
        item.Version.Should().Be(1);

        item.ConfirmReservation(r1.Value.ReservationId, BaseTime);
        item.Version.Should().Be(2);

        var r2 = item.Reserve(Quantity.Of(10), BaseTime);
        item.Version.Should().Be(3);

        item.CancelReservation(r2.Value.ReservationId, BaseTime);
        item.Version.Should().Be(4);

        item.Restock(Quantity.Of(5));
        item.Version.Should().Be(5);
    }

    [Fact]
    public void Version_TracksOptimisticConcurrency_Scenario()
    {
        // Simulating two "threads" reading the same version then trying to act.
        // In a real system, the repository would check version on save.
        // Here we just verify version increments make conflicts detectable.

        var item = CreateItem(10);
        var versionBeforeReserve = item.Version; // 0

        item.Reserve(Quantity.Of(5), BaseTime);
        item.Version.Should().NotBe(versionBeforeReserve);

        // A second concurrent operation would have read version 0 as well.
        // On save, the repository would detect version mismatch (expected 0, got 1).
        // This is the optimistic concurrency contract.
    }

    // =====================
    // COMPLEX SCENARIOS
    // =====================

    [Fact]
    public void FullLifecycle_Reserve_Confirm_StockPermanentlyReduced()
    {
        var item = CreateItem(100);

        var r = item.Reserve(Quantity.Of(30), BaseTime);
        item.ConfirmReservation(r.Value.ReservationId, BaseTime);

        item.AvailableQuantity.Value.Should().Be(70);
        item.ReservedQuantity.Value.Should().Be(0);
        // 30 units permanently sold
    }

    [Fact]
    public void FullLifecycle_Reserve_Cancel_StockRestored()
    {
        var item = CreateItem(100);

        var r = item.Reserve(Quantity.Of(30), BaseTime);
        item.CancelReservation(r.Value.ReservationId, BaseTime);

        item.AvailableQuantity.Value.Should().Be(100);
        item.ReservedQuantity.Value.Should().Be(0);
    }

    [Fact]
    public void FullLifecycle_Reserve_Expire_StockRestored()
    {
        var item = CreateItem(100);

        item.Reserve(Quantity.Of(30), BaseTime, TimeSpan.FromMinutes(5));

        var later = BaseTime.AddMinutes(10);
        item.ExpireStaleReservations(later);

        item.AvailableQuantity.Value.Should().Be(100);
        item.ReservedQuantity.Value.Should().Be(0);
    }

    [Fact]
    public void MultipleReservations_MixedOutcomes()
    {
        var item = CreateItem(100);

        var r1 = item.Reserve(Quantity.Of(30), BaseTime);
        var r2 = item.Reserve(Quantity.Of(25), BaseTime, TimeSpan.FromMinutes(5));
        var r3 = item.Reserve(Quantity.Of(20), BaseTime);

        // Confirm r1, let r2 expire, cancel r3
        item.ConfirmReservation(r1.Value.ReservationId, BaseTime);
        item.ExpireStaleReservations(BaseTime.AddMinutes(10));
        item.CancelReservation(r3.Value.ReservationId, BaseTime.AddMinutes(10));

        // r1: 30 confirmed (permanently gone)
        // r2: 25 expired (returned to available)
        // r3: 20 cancelled (returned to available)
        item.AvailableQuantity.Value.Should().Be(70); // 100 - 30
        item.ReservedQuantity.Value.Should().Be(0);
    }

    [Fact]
    public void PartialReserve_ThenExpire_FreesStockForNewReservation()
    {
        var item = CreateItem(5);

        // First: partial reservation (only 5 available of 20 requested)
        var r1 = item.Reserve(Quantity.Of(20), BaseTime, TimeSpan.FromMinutes(5));
        r1.Value.ReservedQuantity.Value.Should().Be(5);
        r1.Value.Shortfall.Value.Should().Be(15);

        // All stock reserved now
        item.AvailableQuantity.Value.Should().Be(0);

        // After expiry, stock comes back
        var later = BaseTime.AddMinutes(10);
        item.ExpireStaleReservations(later);
        item.AvailableQuantity.Value.Should().Be(5);

        // New reservation can now proceed
        var r2 = item.Reserve(Quantity.Of(3), later);
        r2.IsSuccess.Should().BeTrue();
        r2.Value.ReservedQuantity.Value.Should().Be(3);
    }
}
