namespace SubscriptionManagement.Domain;

public sealed class Subscription
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Plan Plan { get; private set; }
    public SubscriptionState State { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime CurrentPeriodStartUtc { get; private set; }
    public DateTime CurrentPeriodEndUtc { get; private set; }
    public DateTime? TrialEndsAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? PastDueSinceUtc { get; private set; }

    /// <summary>
    /// When set, a downgrade to this plan takes effect at the end of the current billing period.
    /// </summary>
    public Plan? PendingDowngradePlan { get; private set; }

    private static readonly TimeSpan TrialDuration = TimeSpan.FromDays(14);
    private static readonly TimeSpan CoolingOffPeriod = TimeSpan.FromDays(14);
    private static readonly TimeSpan PastDueGracePeriod = TimeSpan.FromDays(30);

    private Subscription() { } // EF Core / serialization

    /// <summary>
    /// Creates a new subscription in Trial state.
    /// </summary>
    public static Subscription StartTrial(Guid customerId, Plan plan, DateTime utcNow)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Plan = plan,
            State = SubscriptionState.Trial,
            CreatedAtUtc = utcNow,
            CurrentPeriodStartUtc = utcNow,
            CurrentPeriodEndUtc = utcNow.Add(TrialDuration),
            TrialEndsAtUtc = utcNow.Add(TrialDuration),
        };
    }

    /// <summary>
    /// Creates a new subscription directly in Active state (no trial).
    /// </summary>
    public static Subscription CreateActive(Guid customerId, Plan plan, DateTime utcNow)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Plan = plan,
            State = SubscriptionState.Active,
            CreatedAtUtc = utcNow,
            CurrentPeriodStartUtc = utcNow,
            CurrentPeriodEndUtc = utcNow.AddMonths(1),
        };
    }

    /// <summary>
    /// Activates the subscription after trial ends.
    /// </summary>
    public Result<Subscription> Activate(DateTime utcNow)
    {
        if (State != SubscriptionState.Trial)
            return Result<Subscription>.Failure("Only trial subscriptions can be activated.");

        State = SubscriptionState.Active;
        CurrentPeriodStartUtc = utcNow;
        CurrentPeriodEndUtc = utcNow.AddMonths(1);
        TrialEndsAtUtc = null;

        return this;
    }

    /// <summary>
    /// Upgrade to a higher-tier plan. Takes effect immediately.
    /// </summary>
    public Result<Subscription> Upgrade(Plan newPlan, DateTime utcNow)
    {
        if (State == SubscriptionState.Cancelled)
            return Result<Subscription>.Failure("Cannot upgrade a cancelled subscription.");

        if (!newPlan.IsUpgradeFrom(Plan))
            return Result<Subscription>.Failure($"Plan '{newPlan.Name}' is not an upgrade from '{Plan.Name}'.");

        Plan = newPlan;
        PendingDowngradePlan = null; // Clear any pending downgrade

        // Upgrades take effect immediately — reset billing period
        CurrentPeriodStartUtc = utcNow;
        CurrentPeriodEndUtc = utcNow.AddMonths(1);

        return this;
    }

    /// <summary>
    /// Downgrade to a lower-tier plan. Takes effect at end of current billing period.
    /// Cannot downgrade during trial.
    /// </summary>
    public Result<Subscription> Downgrade(Plan newPlan)
    {
        if (State == SubscriptionState.Trial)
            return Result<Subscription>.Failure("Cannot downgrade during trial period.");

        if (State == SubscriptionState.Cancelled)
            return Result<Subscription>.Failure("Cannot downgrade a cancelled subscription.");

        if (!newPlan.IsDowngradeFrom(Plan))
            return Result<Subscription>.Failure($"Plan '{newPlan.Name}' is not a downgrade from '{Plan.Name}'.");

        PendingDowngradePlan = newPlan;

        return this;
    }

    /// <summary>
    /// Applies a pending downgrade. Called when the current billing period ends.
    /// </summary>
    public Result<Subscription> ApplyPendingDowngrade(DateTime utcNow)
    {
        if (PendingDowngradePlan is null)
            return Result<Subscription>.Failure("No pending downgrade to apply.");

        Plan = PendingDowngradePlan;
        PendingDowngradePlan = null;
        CurrentPeriodStartUtc = utcNow;
        CurrentPeriodEndUtc = utcNow.AddMonths(1);

        return this;
    }

    /// <summary>
    /// Cancels the subscription. Enters a 14-day cooling-off period during which it can be reactivated.
    /// </summary>
    public Result<Subscription> Cancel(DateTime utcNow)
    {
        if (State == SubscriptionState.Cancelled)
            return Result<Subscription>.Failure("Subscription is already cancelled.");

        State = SubscriptionState.Cancelled;
        CancelledAtUtc = utcNow;
        PendingDowngradePlan = null;

        return this;
    }

    /// <summary>
    /// Reactivates a cancelled subscription within the cooling-off period.
    /// </summary>
    public Result<Subscription> Reactivate(DateTime utcNow)
    {
        if (State != SubscriptionState.Cancelled)
            return Result<Subscription>.Failure("Only cancelled subscriptions can be reactivated.");

        if (CancelledAtUtc is null)
            return Result<Subscription>.Failure("Cancellation date is missing.");

        if (utcNow - CancelledAtUtc.Value > CoolingOffPeriod)
            return Result<Subscription>.Failure("Cooling-off period has expired. Cannot reactivate.");

        State = SubscriptionState.Active;
        CancelledAtUtc = null;
        CurrentPeriodStartUtc = utcNow;
        CurrentPeriodEndUtc = utcNow.AddMonths(1);

        return this;
    }

    /// <summary>
    /// Marks the subscription as past due (e.g., payment failed).
    /// </summary>
    public Result<Subscription> MarkPastDue(DateTime utcNow)
    {
        if (State != SubscriptionState.Active)
            return Result<Subscription>.Failure("Only active subscriptions can be marked as past due.");

        State = SubscriptionState.PastDue;
        PastDueSinceUtc = utcNow;

        return this;
    }

    /// <summary>
    /// Resolves a past-due subscription back to active (e.g., payment succeeded).
    /// </summary>
    public Result<Subscription> ResolvePastDue(DateTime utcNow)
    {
        if (State != SubscriptionState.PastDue)
            return Result<Subscription>.Failure("Subscription is not past due.");

        State = SubscriptionState.Active;
        PastDueSinceUtc = null;
        CurrentPeriodStartUtc = utcNow;
        CurrentPeriodEndUtc = utcNow.AddMonths(1);

        return this;
    }

    /// <summary>
    /// Checks if the trial has expired.
    /// </summary>
    public bool IsTrialExpired(DateTime utcNow) =>
        State == SubscriptionState.Trial && TrialEndsAtUtc.HasValue && utcNow >= TrialEndsAtUtc.Value;

    /// <summary>
    /// Checks if a past-due subscription should be auto-cancelled (30 days).
    /// </summary>
    public bool ShouldAutoCancelPastDue(DateTime utcNow) =>
        State == SubscriptionState.PastDue && PastDueSinceUtc.HasValue
        && (utcNow - PastDueSinceUtc.Value) >= PastDueGracePeriod;

    /// <summary>
    /// Checks if the cooling-off period is still active.
    /// </summary>
    public bool IsInCoolingOffPeriod(DateTime utcNow) =>
        State == SubscriptionState.Cancelled && CancelledAtUtc.HasValue
        && (utcNow - CancelledAtUtc.Value) <= CoolingOffPeriod;

    /// <summary>
    /// Checks if the current billing period has ended and a pending downgrade should be applied.
    /// </summary>
    public bool HasPendingDowngradeReady(DateTime utcNow) =>
        PendingDowngradePlan is not null && utcNow >= CurrentPeriodEndUtc;
}
