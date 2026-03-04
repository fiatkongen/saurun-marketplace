namespace SubscriptionManagement.Domain;

public sealed class Subscription
{
    public static readonly TimeSpan TrialDuration = TimeSpan.FromDays(14);
    public static readonly TimeSpan CancellationCoolingOffPeriod = TimeSpan.FromDays(14);
    public static readonly TimeSpan PastDueGracePeriod = TimeSpan.FromDays(30);

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Plan CurrentPlan { get; private set; } = null!;
    public Plan? PendingPlan { get; private set; }
    public DateTime? PendingPlanEffectiveDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? TrialEndsAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? CancellationEffectiveAtUtc { get; private set; }
    public DateTime? PastDueSinceUtc { get; private set; }
    public DateTime CurrentBillingPeriodEndUtc { get; private set; }

    private Subscription() { }

    public static Subscription StartTrial(Guid customerId, Plan plan, DateTime utcNow)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CurrentPlan = plan,
            Status = SubscriptionStatus.Trial,
            CreatedAtUtc = utcNow,
            TrialEndsAtUtc = utcNow.Add(TrialDuration),
            CurrentBillingPeriodEndUtc = utcNow.Add(TrialDuration)
        };
    }

    public Result<Subscription> Activate(DateTime utcNow)
    {
        if (Status != SubscriptionStatus.Trial)
            return Result<Subscription>.Failure("Only trial subscriptions can be activated.");

        if (TrialEndsAtUtc.HasValue && utcNow < TrialEndsAtUtc.Value)
            return Result<Subscription>.Failure("Trial period has not ended yet.");

        Status = SubscriptionStatus.Active;
        TrialEndsAtUtc = null;
        CurrentBillingPeriodEndUtc = utcNow.AddMonths(1);
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> ChangePlan(Plan newPlan, DateTime utcNow)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result<Subscription>.Failure("Cannot change plan on a cancelled subscription.");

        if (newPlan.Type == CurrentPlan.Type)
            return Result<Subscription>.Failure("Already on this plan.");

        if (Status == SubscriptionStatus.Trial && newPlan.IsDowngradeFrom(CurrentPlan))
            return Result<Subscription>.Failure("Cannot downgrade during trial period.");

        if (newPlan.IsUpgradeFrom(CurrentPlan))
        {
            // Upgrades take effect immediately
            CurrentPlan = newPlan;
            PendingPlan = null;
            PendingPlanEffectiveDate = null;
            return Result<Subscription>.Success(this);
        }

        // Downgrade: schedule for end of billing period
        PendingPlan = newPlan;
        PendingPlanEffectiveDate = CurrentBillingPeriodEndUtc;
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> ApplyPendingPlanChange(DateTime utcNow)
    {
        if (PendingPlan is null || PendingPlanEffectiveDate is null)
            return Result<Subscription>.Failure("No pending plan change.");

        if (utcNow < PendingPlanEffectiveDate.Value)
            return Result<Subscription>.Failure("Pending plan change is not yet effective.");

        CurrentPlan = PendingPlan;
        PendingPlan = null;
        PendingPlanEffectiveDate = null;
        CurrentBillingPeriodEndUtc = utcNow.AddMonths(1);
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> Cancel(DateTime utcNow)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result<Subscription>.Failure("Subscription is already cancelled.");

        Status = SubscriptionStatus.Cancelled;
        CancelledAtUtc = utcNow;
        CancellationEffectiveAtUtc = utcNow.Add(CancellationCoolingOffPeriod);
        PendingPlan = null;
        PendingPlanEffectiveDate = null;
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> Reactivate(DateTime utcNow)
    {
        if (Status != SubscriptionStatus.Cancelled)
            return Result<Subscription>.Failure("Only cancelled subscriptions can be reactivated.");

        if (CancellationEffectiveAtUtc.HasValue && utcNow >= CancellationEffectiveAtUtc.Value)
            return Result<Subscription>.Failure("Cooling-off period has expired. Cannot reactivate.");

        Status = SubscriptionStatus.Active;
        CancelledAtUtc = null;
        CancellationEffectiveAtUtc = null;
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> MarkPastDue(DateTime utcNow)
    {
        if (Status != SubscriptionStatus.Active)
            return Result<Subscription>.Failure("Only active subscriptions can be marked as past due.");

        Status = SubscriptionStatus.PastDue;
        PastDueSinceUtc = utcNow;
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> ResolvePayment(DateTime utcNow)
    {
        if (Status != SubscriptionStatus.PastDue)
            return Result<Subscription>.Failure("Only past-due subscriptions can have payment resolved.");

        Status = SubscriptionStatus.Active;
        PastDueSinceUtc = null;
        CurrentBillingPeriodEndUtc = utcNow.AddMonths(1);
        return Result<Subscription>.Success(this);
    }

    public Result<Subscription> AutoCancelIfPastDueExpired(DateTime utcNow)
    {
        if (Status != SubscriptionStatus.PastDue)
            return Result<Subscription>.Failure("Subscription is not past due.");

        if (!PastDueSinceUtc.HasValue)
            return Result<Subscription>.Failure("Past due date is not set.");

        if (utcNow < PastDueSinceUtc.Value.Add(PastDueGracePeriod))
            return Result<Subscription>.Failure("Past due grace period has not expired.");

        Status = SubscriptionStatus.Cancelled;
        CancelledAtUtc = utcNow;
        CancellationEffectiveAtUtc = utcNow; // Immediate, no cooling-off for auto-cancel
        return Result<Subscription>.Success(this);
    }

    public bool IsInCoolingOffPeriod(DateTime utcNow)
    {
        return Status == SubscriptionStatus.Cancelled
               && CancellationEffectiveAtUtc.HasValue
               && utcNow < CancellationEffectiveAtUtc.Value;
    }

    public bool IsTrialExpired(DateTime utcNow)
    {
        return Status == SubscriptionStatus.Trial
               && TrialEndsAtUtc.HasValue
               && utcNow >= TrialEndsAtUtc.Value;
    }

    public bool IsPastDueExpired(DateTime utcNow)
    {
        return Status == SubscriptionStatus.PastDue
               && PastDueSinceUtc.HasValue
               && utcNow >= PastDueSinceUtc.Value.Add(PastDueGracePeriod);
    }

    public bool HasPendingDowngrade => PendingPlan is not null && PendingPlanEffectiveDate is not null;
}
