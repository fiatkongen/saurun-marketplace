using SubscriptionManagement.Domain;

namespace SubscriptionManagement.Services;

/// <summary>
/// Domain service that orchestrates subscription lifecycle operations including
/// plan changes, state transitions, and time-based processing.
/// </summary>
public sealed class SubscriptionService
{
    private readonly IDateTimeProvider _dateTime;

    public SubscriptionService(IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    /// <summary>
    /// Starts a new trial subscription for a customer.
    /// </summary>
    public Result<Subscription> StartTrial(Guid customerId, Plan plan)
    {
        var subscription = Subscription.StartTrial(customerId, plan, _dateTime.UtcNow);
        return subscription;
    }

    /// <summary>
    /// Creates a new active subscription (no trial).
    /// </summary>
    public Result<Subscription> CreateActive(Guid customerId, Plan plan)
    {
        var subscription = Subscription.CreateActive(customerId, plan, _dateTime.UtcNow);
        return subscription;
    }

    /// <summary>
    /// Changes the plan for a subscription. Automatically determines if this is
    /// an upgrade (immediate) or downgrade (end of period). Returns failure if
    /// the change is not permitted.
    /// </summary>
    public Result<Subscription> ChangePlan(Subscription subscription, Plan newPlan)
    {
        if (newPlan == subscription.Plan)
            return Result<Subscription>.Failure("New plan is the same as the current plan.");

        if (newPlan.IsUpgradeFrom(subscription.Plan))
            return subscription.Upgrade(newPlan, _dateTime.UtcNow);

        // Downgrade
        return subscription.Downgrade(newPlan);
    }

    /// <summary>
    /// Cancels a subscription. Enters the 14-day cooling-off period.
    /// </summary>
    public Result<Subscription> Cancel(Subscription subscription)
    {
        return subscription.Cancel(_dateTime.UtcNow);
    }

    /// <summary>
    /// Reactivates a cancelled subscription within the cooling-off period.
    /// </summary>
    public Result<Subscription> Reactivate(Subscription subscription)
    {
        return subscription.Reactivate(_dateTime.UtcNow);
    }

    /// <summary>
    /// Marks a subscription as past due (e.g., payment failure).
    /// </summary>
    public Result<Subscription> MarkPastDue(Subscription subscription)
    {
        return subscription.MarkPastDue(_dateTime.UtcNow);
    }

    /// <summary>
    /// Resolves a past-due subscription (e.g., payment succeeded).
    /// </summary>
    public Result<Subscription> ResolvePastDue(Subscription subscription)
    {
        return subscription.ResolvePastDue(_dateTime.UtcNow);
    }

    /// <summary>
    /// Processes time-based transitions for a subscription:
    /// - Trial expiry -> Active
    /// - PastDue auto-cancel after 30 days
    /// - Pending downgrade application at period end
    /// Returns the subscription unchanged if no transitions apply.
    /// </summary>
    public Result<Subscription> ProcessTimeBasedTransitions(Subscription subscription)
    {
        var now = _dateTime.UtcNow;

        // Trial -> Active
        if (subscription.IsTrialExpired(now))
        {
            return subscription.Activate(now);
        }

        // PastDue -> Cancelled (auto-cancel after 30 days)
        if (subscription.ShouldAutoCancelPastDue(now))
        {
            return subscription.Cancel(now);
        }

        // Apply pending downgrade at period end
        if (subscription.HasPendingDowngradeReady(now))
        {
            return subscription.ApplyPendingDowngrade(now);
        }

        return subscription;
    }
}
