using NSubstitute;
using SubscriptionManagement.Domain;
using SubscriptionManagement.Services;

namespace SubscriptionManagement.Tests;

public class SubscriptionServiceTests
{
    private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly IDateTimeProvider _dateTime;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _dateTime = Substitute.For<IDateTimeProvider>();
        _dateTime.UtcNow.Returns(Now);
        _sut = new SubscriptionService(_dateTime);
    }

    // ── Trial Creation ───────────────────────────────────────────

    [Fact]
    public void StartTrial_CreatesTrialSubscription()
    {
        var result = _sut.StartTrial(Guid.NewGuid(), Plan.Basic);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Trial, result.Value!.State);
        Assert.Equal(Plan.Basic, result.Value.Plan);
    }

    [Fact]
    public void CreateActive_CreatesActiveSubscription()
    {
        var result = _sut.CreateActive(Guid.NewGuid(), Plan.Enterprise);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, result.Value!.State);
        Assert.Equal(Plan.Enterprise, result.Value.Plan);
    }

    // ── ChangePlan ───────────────────────────────────────────────

    [Fact]
    public void ChangePlan_Upgrade_TakesEffectImmediately()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        _dateTime.UtcNow.Returns(Now.AddDays(10));

        var result = _sut.ChangePlan(sub, Plan.Pro);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan);
        Assert.Equal(Now.AddDays(10), sub.CurrentPeriodStartUtc); // Reset immediately
    }

    [Fact]
    public void ChangePlan_Downgrade_SetsPendingDowngrade()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Enterprise, Now);

        var result = _sut.ChangePlan(sub, Plan.Basic);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, sub.Plan); // Still on Enterprise
        Assert.Equal(Plan.Basic, sub.PendingDowngradePlan); // Pending
    }

    [Fact]
    public void ChangePlan_SamePlan_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);

        var result = _sut.ChangePlan(sub, Plan.Pro);

        Assert.True(result.IsFailure);
        Assert.Contains("same", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChangePlan_DowngradeDuringTrial_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Pro, Now);

        var result = _sut.ChangePlan(sub, Plan.Basic);

        Assert.True(result.IsFailure);
        Assert.Contains("trial", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChangePlan_UpgradeDuringTrial_Succeeds()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        _dateTime.UtcNow.Returns(Now.AddDays(3));

        var result = _sut.ChangePlan(sub, Plan.Enterprise);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, sub.Plan);
    }

    // ── Cancellation / Reactivation ──────────────────────────────

    [Fact]
    public void Cancel_ActiveSubscription_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);

        var result = _sut.Cancel(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Cancelled, sub.State);
    }

    [Fact]
    public void Reactivate_WithinCoolingOff_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);
        _dateTime.UtcNow.Returns(Now.AddDays(10));

        var result = _sut.Reactivate(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
    }

    [Fact]
    public void Reactivate_AfterCoolingOff_Fails()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Basic, Now);
        sub.Cancel(Now);
        _dateTime.UtcNow.Returns(Now.AddDays(15));

        var result = _sut.Reactivate(sub);

        Assert.True(result.IsFailure);
    }

    // ── PastDue ──────────────────────────────────────────────────

    [Fact]
    public void MarkPastDue_ActiveSubscription_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);

        var result = _sut.MarkPastDue(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.PastDue, sub.State);
    }

    [Fact]
    public void ResolvePastDue_PastDueSubscription_Succeeds()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.MarkPastDue(Now);
        _dateTime.UtcNow.Returns(Now.AddDays(5));

        var result = _sut.ResolvePastDue(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
    }

    // ── Time-Based Transitions ───────────────────────────────────

    [Fact]
    public void ProcessTimeBasedTransitions_ExpiredTrial_ActivatesSubscription()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        _dateTime.UtcNow.Returns(Now.AddDays(14));

        var result = _sut.ProcessTimeBasedTransitions(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
    }

    [Fact]
    public void ProcessTimeBasedTransitions_ActiveTrial_NoChange()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        _dateTime.UtcNow.Returns(Now.AddDays(7));

        var result = _sut.ProcessTimeBasedTransitions(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Trial, sub.State);
    }

    [Fact]
    public void ProcessTimeBasedTransitions_PastDue30Days_AutoCancels()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.MarkPastDue(Now);
        _dateTime.UtcNow.Returns(Now.AddDays(30));

        var result = _sut.ProcessTimeBasedTransitions(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Cancelled, sub.State);
    }

    [Fact]
    public void ProcessTimeBasedTransitions_PastDue29Days_NoChange()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Pro, Now);
        sub.MarkPastDue(Now);
        _dateTime.UtcNow.Returns(Now.AddDays(29));

        var result = _sut.ProcessTimeBasedTransitions(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.PastDue, sub.State);
    }

    [Fact]
    public void ProcessTimeBasedTransitions_PendingDowngradeAtPeriodEnd_AppliesDowngrade()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Downgrade(Plan.Basic);
        var periodEnd = sub.CurrentPeriodEndUtc;
        _dateTime.UtcNow.Returns(periodEnd);

        var result = _sut.ProcessTimeBasedTransitions(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Basic, sub.Plan);
        Assert.Null(sub.PendingDowngradePlan);
    }

    [Fact]
    public void ProcessTimeBasedTransitions_PendingDowngradeBeforePeriodEnd_NoChange()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Downgrade(Plan.Basic);
        _dateTime.UtcNow.Returns(Now.AddDays(15));

        var result = _sut.ProcessTimeBasedTransitions(sub);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, sub.Plan);
        Assert.Equal(Plan.Basic, sub.PendingDowngradePlan);
    }

    // ── Full Lifecycle Integration ───────────────────────────────

    [Fact]
    public void FullLifecycle_TrialToActiveToUpgradeToCancel()
    {
        // Start trial
        var result = _sut.StartTrial(Guid.NewGuid(), Plan.Basic);
        Assert.True(result.IsSuccess);
        var sub = result.Value!;

        // Trial expires -> activate
        _dateTime.UtcNow.Returns(Now.AddDays(14));
        result = _sut.ProcessTimeBasedTransitions(sub);
        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);

        // Upgrade to Pro
        _dateTime.UtcNow.Returns(Now.AddDays(20));
        result = _sut.ChangePlan(sub, Plan.Pro);
        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan);

        // Cancel
        _dateTime.UtcNow.Returns(Now.AddDays(25));
        result = _sut.Cancel(sub);
        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Cancelled, sub.State);

        // Reactivate within cooling-off
        _dateTime.UtcNow.Returns(Now.AddDays(30));
        result = _sut.Reactivate(sub);
        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Active, sub.State);
    }

    [Fact]
    public void FullLifecycle_PastDueToAutoCancel()
    {
        var sub = Subscription.CreateActive(Guid.NewGuid(), Plan.Enterprise, Now);

        // Payment fails
        _dateTime.UtcNow.Returns(Now.AddDays(5));
        var result = _sut.MarkPastDue(sub);
        Assert.True(result.IsSuccess);

        // 30 days later, auto-cancel
        _dateTime.UtcNow.Returns(Now.AddDays(35));
        result = _sut.ProcessTimeBasedTransitions(sub);
        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionState.Cancelled, sub.State);
    }
}
