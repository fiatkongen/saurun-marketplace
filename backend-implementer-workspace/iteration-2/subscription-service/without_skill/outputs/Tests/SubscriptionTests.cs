using SubscriptionManagement.Domain;

namespace SubscriptionManagement.Tests;

public class SubscriptionTests
{
    private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    // ── Factory Methods ──────────────────────────────────────────

    [Fact]
    public void StartTrial_CreatesSubscriptionInTrialState()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        Assert.Equal(SubscriptionState.Trial, sub.State);
        Assert.Equal(Plan.Basic, sub.Plan);
        Assert.Equal(Now, sub.CreatedAtUtc);
        Assert.Equal(Now.AddDays(14), sub.TrialEndsAtUtc);
    }

    [Fact]
    public void CreateActive_CreatesSubscriptionInActiveState()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);

        Assert.Equal(SubscriptionState.Active, sub.State);
        Assert.Equal(Plan.Pro, sub.Plan);
        Assert.Null(sub.TrialEndsAtUtc);
        Assert.Equal(Now.AddMonths(1), sub.CurrentPeriodEndUtc);
    }

    // ── Activation ───────────────────────────────────────────────

    [Fact]
    public void Activate_FromTrial_Succeeds()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Activate(Now.AddDays(14));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
        Assert.Null(sub.TrialEndsAtUtc);
    }

    [Fact]
    public void Activate_FromActive_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Activate(Now);

        Assert.True(result.IsFailure);
        Assert.Contains("trial", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    // ── Upgrades ─────────────────────────────────────────────────

    [Fact]
    public void Upgrade_FromBasicToPro_TakesEffectImmediately()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        var upgradeTime = Now.AddDays(10);

        var result = sub.Upgrade(Plan.Pro, upgradeTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan);
        Assert.Equal(upgradeTime, sub.CurrentPeriodStartUtc);
        Assert.Equal(upgradeTime.AddMonths(1), sub.CurrentPeriodEndUtc);
    }

    [Fact]
    public void Upgrade_DuringTrial_Succeeds()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Upgrade(Plan.Enterprise, Now.AddDays(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, sub.Plan);
    }

    [Fact]
    public void Upgrade_WithLowerPlan_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);

        var result = sub.Upgrade(Plan.Basic, Now);

        Assert.True(result.IsFailure);
        Assert.Contains("not an upgrade", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Upgrade_CancelledSubscription_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);

        var result = sub.Upgrade(Plan.Pro, Now);

        Assert.True(result.IsFailure);
        Assert.Contains("cancelled", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Upgrade_ClearsPendingDowngrade()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.Downgrade(Plan.Basic);
        Assert.NotNull(sub.PendingDowngradePlan);

        sub.Upgrade(Plan.Enterprise, Now.AddDays(5));

        Assert.Null(sub.PendingDowngradePlan);
        Assert.Equal(Plan.Enterprise, sub.Plan);
    }

    // ── Downgrades ───────────────────────────────────────────────

    [Fact]
    public void Downgrade_FromProToBasic_SetsPendingDowngrade()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);

        var result = sub.Downgrade(Plan.Basic);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan); // Still on Pro
        Assert.Equal(Plan.Basic, sub.PendingDowngradePlan);
    }

    [Fact]
    public void Downgrade_DuringTrial_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Pro, Now);

        var result = sub.Downgrade(Plan.Basic);

        Assert.True(result.IsFailure);
        Assert.Contains("trial", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Downgrade_WithHigherPlan_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Downgrade(Plan.Pro);

        Assert.True(result.IsFailure);
        Assert.Contains("not a downgrade", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Downgrade_CancelledSubscription_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.Cancel(Now);

        var result = sub.Downgrade(Plan.Basic);

        Assert.True(result.IsFailure);
    }

    // ── Apply Pending Downgrade ──────────────────────────────────

    [Fact]
    public void ApplyPendingDowngrade_WithPendingPlan_AppliesAndResetsperiod()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Downgrade(Plan.Basic);
        var periodEnd = sub.CurrentPeriodEndUtc;

        var result = sub.ApplyPendingDowngrade(periodEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Basic, sub.Plan);
        Assert.Null(sub.PendingDowngradePlan);
        Assert.Equal(periodEnd, sub.CurrentPeriodStartUtc);
        Assert.Equal(periodEnd.AddMonths(1), sub.CurrentPeriodEndUtc);
    }

    [Fact]
    public void ApplyPendingDowngrade_WithoutPendingPlan_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);

        var result = sub.ApplyPendingDowngrade(Now);

        Assert.True(result.IsFailure);
        Assert.Contains("No pending downgrade", result.Error!);
    }

    // ── Cancellation ─────────────────────────────────────────────

    [Fact]
    public void Cancel_ActiveSubscription_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Cancel(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Cancelled, sub.State);
        Assert.Equal(Now, sub.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);

        var result = sub.Cancel(Now);

        Assert.True(result.IsFailure);
        Assert.Contains("already cancelled", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancel_ClearsPendingDowngrade()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.Downgrade(Plan.Basic);

        sub.Cancel(Now);

        Assert.Null(sub.PendingDowngradePlan);
    }

    // ── Reactivation ─────────────────────────────────────────────

    [Fact]
    public void Reactivate_WithinCoolingOff_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);
        var reactivateTime = Now.AddDays(10); // Within 14-day cooling off

        var result = sub.Reactivate(reactivateTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
        Assert.Null(sub.CancelledAtUtc);
    }

    [Fact]
    public void Reactivate_AfterCoolingOff_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);
        var tooLate = Now.AddDays(15); // Past 14-day cooling off

        var result = sub.Reactivate(tooLate);

        Assert.True(result.IsFailure);
        Assert.Contains("Cooling-off period has expired", result.Error!);
    }

    [Fact]
    public void Reactivate_ExactlyAtCoolingOffBoundary_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);
        var exactBoundary = Now.AddDays(14);

        var result = sub.Reactivate(exactBoundary);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Reactivate_NonCancelledSubscription_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Reactivate(Now);

        Assert.True(result.IsFailure);
    }

    // ── PastDue ──────────────────────────────────────────────────

    [Fact]
    public void MarkPastDue_ActiveSubscription_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.MarkPastDue(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.PastDue, sub.State);
        Assert.Equal(Now, sub.PastDueSinceUtc);
    }

    [Fact]
    public void MarkPastDue_TrialSubscription_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.MarkPastDue(Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ResolvePastDue_PastDueSubscription_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.MarkPastDue(Now);
        var resolveTime = Now.AddDays(5);

        var result = sub.ResolvePastDue(resolveTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
        Assert.Null(sub.PastDueSinceUtc);
    }

    [Fact]
    public void ResolvePastDue_ActiveSubscription_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.ResolvePastDue(Now);

        Assert.True(result.IsFailure);
    }

    // ── Query Methods ────────────────────────────────────────────

    [Fact]
    public void IsTrialExpired_BeforeExpiry_ReturnsFalse()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        Assert.False(sub.IsTrialExpired(Now.AddDays(13)));
    }

    [Fact]
    public void IsTrialExpired_AtExpiry_ReturnsTrue()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        Assert.True(sub.IsTrialExpired(Now.AddDays(14)));
    }

    [Fact]
    public void ShouldAutoCancelPastDue_Before30Days_ReturnsFalse()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.MarkPastDue(Now);

        Assert.False(sub.ShouldAutoCancelPastDue(Now.AddDays(29)));
    }

    [Fact]
    public void ShouldAutoCancelPastDue_At30Days_ReturnsTrue()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.MarkPastDue(Now);

        Assert.True(sub.ShouldAutoCancelPastDue(Now.AddDays(30)));
    }

    [Fact]
    public void IsInCoolingOffPeriod_WithinPeriod_ReturnsTrue()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);

        Assert.True(sub.IsInCoolingOffPeriod(Now.AddDays(10)));
    }

    [Fact]
    public void IsInCoolingOffPeriod_AfterPeriod_ReturnsFalse()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);

        Assert.False(sub.IsInCoolingOffPeriod(Now.AddDays(15)));
    }

    [Fact]
    public void HasPendingDowngradeReady_BeforePeriodEnd_ReturnsFalse()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.Downgrade(Plan.Basic);

        Assert.False(sub.HasPendingDowngradeReady(Now.AddDays(15))); // Period is 1 month
    }

    [Fact]
    public void HasPendingDowngradeReady_AtPeriodEnd_ReturnsTrue()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.Downgrade(Plan.Basic);

        Assert.True(sub.HasPendingDowngradeReady(sub.CurrentPeriodEndUtc));
    }
}
