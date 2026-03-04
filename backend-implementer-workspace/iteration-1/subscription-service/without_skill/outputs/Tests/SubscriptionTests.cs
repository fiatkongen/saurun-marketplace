using SubscriptionManagement.Domain;
using Xunit;

namespace SubscriptionManagement.Tests;

public class SubscriptionTests
{
    private static readonly DateTime Now = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void StartTrial_Creates_Subscription_In_Trial_Status()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Pro, Now);

        Assert.Equal(SubscriptionStatus.Trial, sub.Status);
        Assert.Equal(Plan.Pro, sub.CurrentPlan);
        Assert.Equal(Now.AddDays(14), sub.TrialEndsAtUtc);
    }

    [Fact]
    public void Activate_Succeeds_After_Trial_Expires()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        var afterTrial = Now.AddDays(14);

        var result = sub.Activate(afterTrial);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.Null(sub.TrialEndsAtUtc);
    }

    [Fact]
    public void Activate_Fails_Before_Trial_Expires()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.Activate(Now.AddDays(7));

        Assert.True(result.IsFailure);
        Assert.Contains("not ended yet", result.Error);
    }

    [Fact]
    public void Activate_Fails_For_Non_Trial_Subscription()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));

        var result = sub.Activate(Now.AddDays(15));

        Assert.True(result.IsFailure);
    }

    // --- Plan Change Tests ---

    [Fact]
    public void ChangePlan_Upgrade_Takes_Effect_Immediately()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));

        var result = sub.ChangePlan(Plan.Pro, Now.AddDays(14));

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.CurrentPlan);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void ChangePlan_Downgrade_Scheduled_At_Billing_Period_End()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Activate(Now.AddDays(14));
        var activationTime = Now.AddDays(14);

        var result = sub.ChangePlan(Plan.Basic, activationTime.AddDays(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, sub.CurrentPlan); // Still on Enterprise
        Assert.Equal(Plan.Basic, sub.PendingPlan);
        Assert.NotNull(sub.PendingPlanEffectiveDate);
    }

    [Fact]
    public void ChangePlan_Cannot_Downgrade_During_Trial()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Pro, Now);

        var result = sub.ChangePlan(Plan.Basic, Now.AddDays(3));

        Assert.True(result.IsFailure);
        Assert.Contains("Cannot downgrade during trial", result.Error);
    }

    [Fact]
    public void ChangePlan_Can_Upgrade_During_Trial()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.ChangePlan(Plan.Enterprise, Now.AddDays(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, sub.CurrentPlan);
    }

    [Fact]
    public void ChangePlan_Fails_On_Same_Plan()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));

        var result = sub.ChangePlan(Plan.Basic, Now.AddDays(15));

        Assert.True(result.IsFailure);
        Assert.Contains("Already on this plan", result.Error);
    }

    [Fact]
    public void ChangePlan_Fails_On_Cancelled_Subscription()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(15));

        var result = sub.ChangePlan(Plan.Pro, Now.AddDays(16));

        Assert.True(result.IsFailure);
        Assert.Contains("cancelled", result.Error);
    }

    // --- Pending Downgrade Application ---

    [Fact]
    public void ApplyPendingPlanChange_Succeeds_After_Effective_Date()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Activate(Now.AddDays(14));
        sub.ChangePlan(Plan.Basic, Now.AddDays(15));

        var effectiveDate = sub.PendingPlanEffectiveDate!.Value;
        var result = sub.ApplyPendingPlanChange(effectiveDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Basic, sub.CurrentPlan);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public void ApplyPendingPlanChange_Fails_Before_Effective_Date()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Activate(Now.AddDays(14));
        sub.ChangePlan(Plan.Basic, Now.AddDays(15));

        var result = sub.ApplyPendingPlanChange(Now.AddDays(16));

        Assert.True(result.IsFailure);
        Assert.Contains("not yet effective", result.Error);
    }

    // --- Cancellation & Reactivation Tests ---

    [Fact]
    public void Cancel_Sets_CoolingOff_Period()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        var cancelTime = Now.AddDays(20);

        var result = sub.Cancel(cancelTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        Assert.Equal(cancelTime.AddDays(14), sub.CancellationEffectiveAtUtc);
    }

    [Fact]
    public void Cancel_Clears_Pending_Downgrade()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Activate(Now.AddDays(14));
        sub.ChangePlan(Plan.Basic, Now.AddDays(15));
        Assert.True(sub.HasPendingDowngrade);

        sub.Cancel(Now.AddDays(16));

        Assert.Null(sub.PendingPlan);
        Assert.Null(sub.PendingPlanEffectiveDate);
    }

    [Fact]
    public void Reactivate_During_CoolingOff_Succeeds()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(20));

        var result = sub.Reactivate(Now.AddDays(25)); // Within 14-day cooling off

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.Null(sub.CancelledAtUtc);
    }

    [Fact]
    public void Reactivate_After_CoolingOff_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(20));

        var result = sub.Reactivate(Now.AddDays(35)); // After 14-day cooling off

        Assert.True(result.IsFailure);
        Assert.Contains("Cooling-off period has expired", result.Error);
    }

    [Fact]
    public void Reactivate_Non_Cancelled_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));

        var result = sub.Reactivate(Now.AddDays(15));

        Assert.True(result.IsFailure);
    }

    // --- Past Due Tests ---

    [Fact]
    public void MarkPastDue_Succeeds_For_Active()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));

        var result = sub.MarkPastDue(Now.AddDays(45));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.PastDue, sub.Status);
        Assert.Equal(Now.AddDays(45), sub.PastDueSinceUtc);
    }

    [Fact]
    public void MarkPastDue_Fails_For_Non_Active()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        var result = sub.MarkPastDue(Now.AddDays(5));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ResolvePayment_Returns_To_Active()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.MarkPastDue(Now.AddDays(45));

        var result = sub.ResolvePayment(Now.AddDays(50));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.Null(sub.PastDueSinceUtc);
    }

    [Fact]
    public void AutoCancel_After_30_Days_PastDue()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        var pastDueDate = Now.AddDays(45);
        sub.MarkPastDue(pastDueDate);

        var result = sub.AutoCancelIfPastDueExpired(pastDueDate.AddDays(30));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public void AutoCancel_Before_30_Days_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        var pastDueDate = Now.AddDays(45);
        sub.MarkPastDue(pastDueDate);

        var result = sub.AutoCancelIfPastDueExpired(pastDueDate.AddDays(15));

        Assert.True(result.IsFailure);
        Assert.Contains("grace period has not expired", result.Error);
    }

    // --- Query Helper Tests ---

    [Fact]
    public void IsInCoolingOffPeriod_True_During_Period()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(20));

        Assert.True(sub.IsInCoolingOffPeriod(Now.AddDays(25)));
    }

    [Fact]
    public void IsInCoolingOffPeriod_False_After_Period()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(20));

        Assert.False(sub.IsInCoolingOffPeriod(Now.AddDays(35)));
    }

    [Fact]
    public void IsTrialExpired_Correct()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);

        Assert.False(sub.IsTrialExpired(Now.AddDays(7)));
        Assert.True(sub.IsTrialExpired(Now.AddDays(14)));
        Assert.True(sub.IsTrialExpired(Now.AddDays(15)));
    }
}
