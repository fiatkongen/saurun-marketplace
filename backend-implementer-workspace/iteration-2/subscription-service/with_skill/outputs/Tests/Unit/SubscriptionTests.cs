using SubscriptionManagement.Domain.Entities;
using SubscriptionManagement.Domain.Events;
using SubscriptionManagement.Domain.ValueObjects;

namespace SubscriptionManagement.Tests.Unit;

public class SubscriptionTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_ReturnsTrialSubscription()
    {
        var now = DateTime.UtcNow;
        var result = Subscription.Create(CustomerId, Plan.Basic, now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Trial, result.Value.Status);
        Assert.Equal(Plan.Basic, result.Value.Plan);
        Assert.Equal(CustomerId, result.Value.CustomerId);
    }

    [Fact]
    public void Create_SetsTrialEndDate_14DaysFromNow()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = Subscription.Create(CustomerId, Plan.Basic, now);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), result.Value.TrialEndDate);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ReturnsFailure()
    {
        var result = Subscription.Create(Guid.Empty, Plan.Basic, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Contains("Customer ID", result.Error);
    }

    [Fact]
    public void Create_RaisesSubscriptionCreatedEvent()
    {
        var result = Subscription.Create(CustomerId, Plan.Basic, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        var domainEvent = Assert.Single(result.Value.DomainEvents);
        Assert.IsType<SubscriptionCreatedEvent>(domainEvent);
    }

    [Fact]
    public void Activate_DuringTrial_TransitionsToActive()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;

        var result = sub.Activate(now.AddDays(14));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));

        var result = sub.Activate(now.AddDays(15));

        Assert.True(result.IsFailure);
        Assert.Contains("already active", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancel_ActiveSubscription_TransitionsToCancelledWithCoolingOff()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));

        var cancelDate = now.AddDays(20);
        var result = sub.Cancel(cancelDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        Assert.Equal(cancelDate.AddDays(14), sub.CancellationCoolingOffEndDate);
    }

    [Fact]
    public void Cancel_TrialSubscription_TransitionsToCancelled()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;

        var result = sub.Cancel(now.AddDays(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Cancel(now.AddDays(1));

        var result = sub.Cancel(now.AddDays(2));

        Assert.True(result.IsFailure);
        Assert.Contains("already cancelled", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Reactivate_WithinCoolingOffPeriod_TransitionsToActive()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));
        sub.Cancel(now.AddDays(20));

        // Reactivate 5 days after cancellation (within 14-day cooling-off)
        var reactivateDate = now.AddDays(25);
        var result = sub.Reactivate(reactivateDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public void Reactivate_AfterCoolingOffPeriod_ReturnsFailure()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));
        sub.Cancel(now.AddDays(20));

        // Try to reactivate 15 days after cancellation (beyond 14-day cooling-off)
        var reactivateDate = now.AddDays(35);
        var result = sub.Reactivate(reactivateDate);

        Assert.True(result.IsFailure);
        Assert.Contains("cooling-off", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Reactivate_WhenNotCancelled_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));

        var result = sub.Reactivate(now.AddDays(15));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MarkPastDue_ActiveSubscription_TransitionsToPastDue()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));

        var result = sub.MarkPastDue(now.AddDays(44));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.PastDue, sub.Status);
    }

    [Fact]
    public void MarkPastDue_SetsAutoCancel30DaysOut()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));

        var pastDueDate = now.AddDays(44);
        sub.MarkPastDue(pastDueDate);

        Assert.Equal(pastDueDate.AddDays(30), sub.AutoCancelDate);
    }

    [Fact]
    public void MarkPastDue_NonActiveSubscription_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;

        var result = sub.MarkPastDue(now.AddDays(5));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ChangePlan_UpgradeDuringTrial_Succeeds()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;

        var result = sub.ChangePlan(Plan.Pro, now.AddDays(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan);
    }

    [Fact]
    public void ChangePlan_DowngradeDuringTrial_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Pro, now).Value;

        var result = sub.ChangePlan(Plan.Basic, now.AddDays(3));

        Assert.True(result.IsFailure);
        Assert.Contains("downgrade", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trial", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChangePlan_UpgradeWhenActive_TakesEffectImmediately()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));

        var result = sub.ChangePlan(Plan.Pro, now.AddDays(15));

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void ChangePlan_DowngradeWhenActive_SchedulesForBillingPeriodEnd()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Pro, now).Value;
        sub.Activate(now.AddDays(14));

        var changeDate = now.AddDays(20);
        var result = sub.ChangePlan(Plan.Basic, changeDate);

        Assert.True(result.IsSuccess);
        // Current plan stays Pro
        Assert.Equal(Plan.Pro, sub.Plan);
        // Downgrade is pending
        Assert.Equal(Plan.Basic, sub.PendingPlan);
    }

    [Fact]
    public void ChangePlan_ToCancelledSubscription_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Cancel(now.AddDays(1));

        var result = sub.ChangePlan(Plan.Pro, now.AddDays(2));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ChangePlan_ToSamePlan_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;

        var result = sub.ChangePlan(Plan.Basic, now.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Contains("same plan", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyPendingPlanChange_WithPendingPlan_AppliesChange()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var sub = Subscription.Create(CustomerId, Plan.Pro, now).Value;
        sub.Activate(now.AddDays(14));
        sub.ChangePlan(Plan.Basic, now.AddDays(20)); // Schedules downgrade

        var result = sub.ApplyPendingPlanChange();

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Basic, sub.Plan);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void ApplyPendingPlanChange_WithNoPendingPlan_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;

        var result = sub.ApplyPendingPlanChange();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Cancel_RaisesSubscriptionCancelledEvent()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));
        sub.ClearDomainEvents();

        sub.Cancel(now.AddDays(20));

        var domainEvent = Assert.Single(sub.DomainEvents);
        Assert.IsType<SubscriptionCancelledEvent>(domainEvent);
    }

    [Fact]
    public void ChangePlan_Upgrade_RaisesPlanChangedEvent()
    {
        var now = DateTime.UtcNow;
        var sub = Subscription.Create(CustomerId, Plan.Basic, now).Value;
        sub.Activate(now.AddDays(14));
        sub.ClearDomainEvents();

        sub.ChangePlan(Plan.Pro, now.AddDays(15));

        var domainEvent = Assert.Single(sub.DomainEvents);
        Assert.IsType<PlanChangedEvent>(domainEvent);
    }
}
