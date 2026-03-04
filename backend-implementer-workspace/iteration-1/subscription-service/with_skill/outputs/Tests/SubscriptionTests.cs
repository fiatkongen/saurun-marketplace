using NSubstitute;
using SubscriptionManagement.Domain.Entities;
using SubscriptionManagement.Domain.Enums;
using SubscriptionManagement.Domain.Events;
using SubscriptionManagement.Domain.ValueObjects;
using Xunit;

namespace SubscriptionManagement.Tests;

public class SubscriptionTests
{
    private static readonly DateTime Now = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    // ============================================================
    // Factory / CreateTrial
    // ============================================================

    [Fact]
    public void CreateTrial_WithValidInputs_ReturnsTrialSubscription()
    {
        var customerId = Guid.NewGuid();
        var result = Subscription.CreateTrial(customerId, Plan.Pro, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Trial, result.Value.Status);
        Assert.Equal(PlanType.Pro, result.Value.CurrentPlan.Type);
    }

    [Fact]
    public void CreateTrial_WithEmptyCustomerId_ReturnsFailure()
    {
        var result = Subscription.CreateTrial(Guid.Empty, Plan.Basic, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Customer ID is required", result.Error);
    }

    [Fact]
    public void CreateTrial_SetsTrialEndDate14DaysFromStart()
    {
        var result = Subscription.CreateTrial(Guid.NewGuid(), Plan.Basic, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now.AddDays(14), result.Value.TrialEndDate);
    }

    [Fact]
    public void CreateTrial_RaisesSubscriptionCreatedEvent()
    {
        var customerId = Guid.NewGuid();
        var result = Subscription.CreateTrial(customerId, Plan.Enterprise, Now);

        Assert.True(result.IsSuccess);
        var @event = Assert.Single(result.Value.DomainEvents);
        var createdEvent = Assert.IsType<SubscriptionCreatedEvent>(@event);
        Assert.Equal(customerId, createdEvent.CustomerId);
        Assert.Equal(PlanType.Enterprise, createdEvent.Plan);
    }

    // ============================================================
    // Activate (Trial -> Active)
    // ============================================================

    [Fact]
    public void Activate_FromTrial_TransitionsToActive()
    {
        var sub = CreateTrialSubscription();

        var result = sub.Activate(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public void Activate_FromTrial_SetsBillingPeriodEnd()
    {
        var sub = CreateTrialSubscription();

        sub.Activate(Now);

        Assert.Equal(Now.AddMonths(1), sub.CurrentBillingPeriodEnd);
    }

    [Fact]
    public void Activate_FromTrial_ClearsTrialEndDate()
    {
        var sub = CreateTrialSubscription();

        sub.Activate(Now);

        Assert.Null(sub.TrialEndDate);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsFailure()
    {
        var sub = CreateActiveSubscription();

        var result = sub.Activate(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Only trial subscriptions can be activated", result.Error);
    }

    [Fact]
    public void Activate_RaisesSubscriptionActivatedEvent()
    {
        var sub = CreateTrialSubscription();
        sub.ClearDomainEvents();

        sub.Activate(Now);

        var @event = Assert.Single(sub.DomainEvents);
        Assert.IsType<SubscriptionActivatedEvent>(@event);
    }

    // ============================================================
    // Cancel
    // ============================================================

    [Fact]
    public void Cancel_FromActive_TransitionsToCancelled()
    {
        var sub = CreateActiveSubscription();

        var result = sub.Cancel(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public void Cancel_SetsCoolingOffPeriodOf14Days()
    {
        var sub = CreateActiveSubscription();

        sub.Cancel(Now);

        Assert.Equal(Now, sub.CancelledDate);
        Assert.Equal(Now.AddDays(14), sub.CoolingOffEndDate);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ReturnsFailure()
    {
        var sub = CreateActiveSubscription();
        sub.Cancel(Now);

        var result = sub.Cancel(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription is already cancelled", result.Error);
    }

    [Fact]
    public void Cancel_ClearsPendingPlanChange()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);
        sub.ChangePlan(Plan.Basic, Now); // downgrade pending
        sub.ClearDomainEvents();

        sub.Cancel(Now);

        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void Cancel_RaisesCancelledEvent()
    {
        var sub = CreateActiveSubscription();
        sub.ClearDomainEvents();

        sub.Cancel(Now);

        var @event = Assert.Single(sub.DomainEvents);
        var cancelledEvent = Assert.IsType<SubscriptionCancelledEvent>(@event);
        Assert.Equal(Now.AddDays(14), cancelledEvent.CoolingOffEndDate);
    }

    // ============================================================
    // Reactivate (within cooling-off period)
    // ============================================================

    [Fact]
    public void Reactivate_WithinCoolingOff_TransitionsToActive()
    {
        var sub = CreateCancelledSubscription();
        var reactivateTime = Now.AddDays(10); // within 14-day cooling-off

        var result = sub.Reactivate(reactivateTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public void Reactivate_WithinCoolingOff_ClearsCancellationFields()
    {
        var sub = CreateCancelledSubscription();

        sub.Reactivate(Now.AddDays(5));

        Assert.Null(sub.CancelledDate);
        Assert.Null(sub.CoolingOffEndDate);
    }

    [Fact]
    public void Reactivate_AfterCoolingOffExpired_ReturnsFailure()
    {
        var sub = CreateCancelledSubscription();
        var afterCoolingOff = Now.AddDays(15); // past 14-day cooling-off

        var result = sub.Reactivate(afterCoolingOff);

        Assert.True(result.IsFailure);
        Assert.Equal("Cooling-off period has expired", result.Error);
    }

    [Fact]
    public void Reactivate_WhenNotCancelled_ReturnsFailure()
    {
        var sub = CreateActiveSubscription();

        var result = sub.Reactivate(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Only cancelled subscriptions can be reactivated", result.Error);
    }

    [Fact]
    public void Reactivate_RaisesReactivatedEvent()
    {
        var sub = CreateCancelledSubscription();
        sub.ClearDomainEvents();

        sub.Reactivate(Now.AddDays(3));

        var @event = Assert.Single(sub.DomainEvents);
        Assert.IsType<SubscriptionReactivatedEvent>(@event);
    }

    // ============================================================
    // ChangePlan - Upgrades (immediate)
    // ============================================================

    [Fact]
    public void ChangePlan_UpgradeFromBasicToPro_TakesEffectImmediately()
    {
        var sub = CreateActiveSubscription(PlanType.Basic);

        var result = sub.ChangePlan(Plan.Pro, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Pro, sub.CurrentPlan.Type);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void ChangePlan_UpgradeFromBasicToEnterprise_TakesEffectImmediately()
    {
        var sub = CreateActiveSubscription(PlanType.Basic);

        var result = sub.ChangePlan(Plan.Enterprise, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Enterprise, sub.CurrentPlan.Type);
    }

    [Fact]
    public void ChangePlan_UpgradeFromProToEnterprise_TakesEffectImmediately()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);

        sub.ChangePlan(Plan.Enterprise, Now);

        Assert.Equal(PlanType.Enterprise, sub.CurrentPlan.Type);
    }

    [Fact]
    public void ChangePlan_Upgrade_RaisesPlanChangedEventWithImmediateTrue()
    {
        var sub = CreateActiveSubscription(PlanType.Basic);
        sub.ClearDomainEvents();

        sub.ChangePlan(Plan.Pro, Now);

        var @event = Assert.Single(sub.DomainEvents);
        var changedEvent = Assert.IsType<PlanChangedEvent>(@event);
        Assert.Equal(PlanType.Basic, changedEvent.OldPlan);
        Assert.Equal(PlanType.Pro, changedEvent.NewPlan);
        Assert.True(changedEvent.Immediate);
    }

    // ============================================================
    // ChangePlan - Downgrades (end of billing period)
    // ============================================================

    [Fact]
    public void ChangePlan_DowngradeFromProToBasic_SetsPendingPlan()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);

        var result = sub.ChangePlan(Plan.Basic, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Pro, sub.CurrentPlan.Type); // still on Pro
        Assert.NotNull(sub.PendingPlan);
        Assert.Equal(PlanType.Basic, sub.PendingPlan!.Type);
    }

    [Fact]
    public void ChangePlan_DowngradeFromEnterpriseToBasic_SetsPendingPlan()
    {
        var sub = CreateActiveSubscription(PlanType.Enterprise);

        sub.ChangePlan(Plan.Basic, Now);

        Assert.Equal(PlanType.Enterprise, sub.CurrentPlan.Type);
        Assert.Equal(PlanType.Basic, sub.PendingPlan!.Type);
    }

    [Fact]
    public void ChangePlan_Downgrade_RaisesPlanChangedEventWithImmediateFalse()
    {
        var sub = CreateActiveSubscription(PlanType.Enterprise);
        sub.ClearDomainEvents();

        sub.ChangePlan(Plan.Pro, Now);

        var @event = Assert.Single(sub.DomainEvents);
        var changedEvent = Assert.IsType<PlanChangedEvent>(@event);
        Assert.False(changedEvent.Immediate);
    }

    // ============================================================
    // ChangePlan - Trial restrictions
    // ============================================================

    [Fact]
    public void ChangePlan_DowngradeDuringTrial_ReturnsFailure()
    {
        var sub = CreateTrialSubscription(PlanType.Pro);

        var result = sub.ChangePlan(Plan.Basic, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot downgrade during trial period", result.Error);
    }

    [Fact]
    public void ChangePlan_UpgradeDuringTrial_Succeeds()
    {
        var sub = CreateTrialSubscription(PlanType.Basic);

        var result = sub.ChangePlan(Plan.Pro, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Pro, sub.CurrentPlan.Type);
    }

    // ============================================================
    // ChangePlan - Edge cases
    // ============================================================

    [Fact]
    public void ChangePlan_ToSamePlan_ReturnsFailure()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);

        var result = sub.ChangePlan(Plan.Pro, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Already on this plan", result.Error);
    }

    [Fact]
    public void ChangePlan_WhenCancelled_ReturnsFailure()
    {
        var sub = CreateCancelledSubscription();

        var result = sub.ChangePlan(Plan.Enterprise, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot change plan on a cancelled subscription", result.Error);
    }

    // ============================================================
    // ApplyPendingPlanChange
    // ============================================================

    [Fact]
    public void ApplyPendingPlanChange_AtEndOfBillingPeriod_AppliesDowngrade()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);
        sub.ChangePlan(Plan.Basic, Now);
        var billingEnd = sub.CurrentBillingPeriodEnd!.Value;

        var result = sub.ApplyPendingPlanChange(billingEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Basic, sub.CurrentPlan.Type);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void ApplyPendingPlanChange_BeforeBillingPeriodEnd_ReturnsFailure()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);
        sub.ChangePlan(Plan.Basic, Now);

        var result = sub.ApplyPendingPlanChange(Now.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal("Billing period has not ended yet", result.Error);
    }

    [Fact]
    public void ApplyPendingPlanChange_WithNoPending_ReturnsFailure()
    {
        var sub = CreateActiveSubscription();

        var result = sub.ApplyPendingPlanChange(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("No pending plan change", result.Error);
    }

    [Fact]
    public void ApplyPendingPlanChange_SetsNewBillingPeriodEnd()
    {
        var sub = CreateActiveSubscription(PlanType.Enterprise);
        sub.ChangePlan(Plan.Basic, Now);
        var billingEnd = sub.CurrentBillingPeriodEnd!.Value;

        sub.ApplyPendingPlanChange(billingEnd);

        Assert.Equal(billingEnd.AddMonths(1), sub.CurrentBillingPeriodEnd);
    }

    // ============================================================
    // PastDue and auto-cancellation
    // ============================================================

    [Fact]
    public void MarkPastDue_FromActive_TransitionsToPastDue()
    {
        var sub = CreateActiveSubscription();

        var result = sub.MarkPastDue(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.PastDue, sub.Status);
        Assert.Equal(Now, sub.PastDueDate);
    }

    [Fact]
    public void MarkPastDue_WhenNotActive_ReturnsFailure()
    {
        var sub = CreateTrialSubscription();

        var result = sub.MarkPastDue(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Only active subscriptions can be marked past due", result.Error);
    }

    [Fact]
    public void MarkPastDue_RaisesPastDueEvent()
    {
        var sub = CreateActiveSubscription();
        sub.ClearDomainEvents();

        sub.MarkPastDue(Now);

        var @event = Assert.Single(sub.DomainEvents);
        Assert.IsType<SubscriptionPastDueEvent>(@event);
    }

    [Fact]
    public void AutoCancelIfPastDueExpired_After30Days_CancelsSubscription()
    {
        var sub = CreatePastDueSubscription();
        var after30Days = Now.AddDays(30);

        var result = sub.AutoCancelIfPastDueExpired(after30Days);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public void AutoCancelIfPastDueExpired_After30Days_HasNoCoolingOff()
    {
        var sub = CreatePastDueSubscription();
        var after30Days = Now.AddDays(30);

        sub.AutoCancelIfPastDueExpired(after30Days);

        Assert.Null(sub.CoolingOffEndDate);
    }

    [Fact]
    public void AutoCancelIfPastDueExpired_Before30Days_ReturnsFailure()
    {
        var sub = CreatePastDueSubscription();
        var before30Days = Now.AddDays(20);

        var result = sub.AutoCancelIfPastDueExpired(before30Days);

        Assert.True(result.IsFailure);
        Assert.Contains("Grace period has not expired", result.Error);
    }

    [Fact]
    public void AutoCancelIfPastDueExpired_WhenNotPastDue_ReturnsFailure()
    {
        var sub = CreateActiveSubscription();

        var result = sub.AutoCancelIfPastDueExpired(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription is not past due", result.Error);
    }

    // ============================================================
    // Trial expiration check
    // ============================================================

    [Fact]
    public void IsTrialExpired_BeforeTrialEnd_ReturnsFalse()
    {
        var sub = CreateTrialSubscription();

        Assert.False(sub.IsTrialExpired(Now.AddDays(10)));
    }

    [Fact]
    public void IsTrialExpired_AtTrialEnd_ReturnsTrue()
    {
        var sub = CreateTrialSubscription();

        Assert.True(sub.IsTrialExpired(Now.AddDays(14)));
    }

    [Fact]
    public void IsTrialExpired_AfterTrialEnd_ReturnsTrue()
    {
        var sub = CreateTrialSubscription();

        Assert.True(sub.IsTrialExpired(Now.AddDays(20)));
    }

    [Fact]
    public void IsTrialExpired_WhenActive_ReturnsFalse()
    {
        var sub = CreateActiveSubscription();

        Assert.False(sub.IsTrialExpired(Now.AddDays(100)));
    }

    // ============================================================
    // Cooling-off expiration check
    // ============================================================

    [Fact]
    public void IsCoolingOffExpired_WithinPeriod_ReturnsFalse()
    {
        var sub = CreateCancelledSubscription();

        Assert.False(sub.IsCoolingOffExpired(Now.AddDays(10)));
    }

    [Fact]
    public void IsCoolingOffExpired_AfterPeriod_ReturnsTrue()
    {
        var sub = CreateCancelledSubscription();

        Assert.True(sub.IsCoolingOffExpired(Now.AddDays(15)));
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static Subscription CreateTrialSubscription(PlanType planType = PlanType.Basic)
    {
        var plan = Plan.FromType(planType).Value;
        return Subscription.CreateTrial(Guid.NewGuid(), plan, Now).Value;
    }

    private static Subscription CreateActiveSubscription(PlanType planType = PlanType.Basic)
    {
        var sub = CreateTrialSubscription(planType);
        sub.Activate(Now);
        sub.ClearDomainEvents();
        return sub;
    }

    private static Subscription CreateCancelledSubscription(PlanType planType = PlanType.Basic)
    {
        var sub = CreateActiveSubscription(planType);
        sub.Cancel(Now);
        sub.ClearDomainEvents();
        return sub;
    }

    private static Subscription CreatePastDueSubscription(PlanType planType = PlanType.Basic)
    {
        var sub = CreateActiveSubscription(planType);
        sub.MarkPastDue(Now);
        sub.ClearDomainEvents();
        return sub;
    }
}
