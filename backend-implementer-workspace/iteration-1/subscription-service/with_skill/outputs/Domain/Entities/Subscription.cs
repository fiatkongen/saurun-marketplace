using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Enums;
using SubscriptionManagement.Domain.Events;
using SubscriptionManagement.Domain.ValueObjects;

namespace SubscriptionManagement.Domain.Entities;

public class Subscription : AggregateRoot<Guid>
{
    public static readonly int TrialDurationDays = 14;
    public static readonly int CoolingOffPeriodDays = 14;
    public static readonly int PastDueGracePeriodDays = 30;

    public Guid CustomerId { get; private set; }
    public Plan CurrentPlan { get; private set; }
    public Plan? PendingPlan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime? CancelledDate { get; private set; }
    public DateTime? CoolingOffEndDate { get; private set; }
    public DateTime? PastDueDate { get; private set; }
    public DateTime? CurrentBillingPeriodEnd { get; private set; }

    private Subscription() { CurrentPlan = null!; } // EF Core

    private Subscription(Guid id, Guid customerId, Plan plan, DateTime now) : base(id)
    {
        CustomerId = customerId;
        CurrentPlan = plan;
        Status = SubscriptionStatus.Trial;
        StartDate = now;
        TrialEndDate = now.AddDays(TrialDurationDays);
    }

    public static Result<Subscription> CreateTrial(Guid customerId, Plan plan, DateTime now)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Subscription>("Customer ID is required");

        var subscription = new Subscription(Guid.NewGuid(), customerId, plan, now);
        subscription.AddDomainEvent(new SubscriptionCreatedEvent(
            subscription.Id, customerId, plan.Type, now));
        return Result.Success(subscription);
    }

    public Result Activate(DateTime now)
    {
        if (Status != SubscriptionStatus.Trial)
            return Result.Failure("Only trial subscriptions can be activated");

        Status = SubscriptionStatus.Active;
        TrialEndDate = null;
        CurrentBillingPeriodEnd = now.AddMonths(1);
        AddDomainEvent(new SubscriptionActivatedEvent(Id, now));
        return Result.Success();
    }

    public Result Cancel(DateTime now)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result.Failure("Subscription is already cancelled");

        CancelledDate = now;
        CoolingOffEndDate = now.AddDays(CoolingOffPeriodDays);
        Status = SubscriptionStatus.Cancelled;
        PendingPlan = null;

        AddDomainEvent(new SubscriptionCancelledEvent(
            Id, now, CoolingOffEndDate.Value, now));
        return Result.Success();
    }

    public Result Reactivate(DateTime now)
    {
        if (Status != SubscriptionStatus.Cancelled)
            return Result.Failure("Only cancelled subscriptions can be reactivated");
        if (CoolingOffEndDate is null || now > CoolingOffEndDate.Value)
            return Result.Failure("Cooling-off period has expired");

        Status = SubscriptionStatus.Active;
        CancelledDate = null;
        CoolingOffEndDate = null;

        AddDomainEvent(new SubscriptionReactivatedEvent(Id, now));
        return Result.Success();
    }

    public Result ChangePlan(Plan newPlan, DateTime now)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result.Failure("Cannot change plan on a cancelled subscription");

        if (newPlan == CurrentPlan)
            return Result.Failure("Already on this plan");

        if (Status == SubscriptionStatus.Trial && newPlan.IsDowngradeFrom(CurrentPlan))
            return Result.Failure("Cannot downgrade during trial period");

        var oldPlan = CurrentPlan;

        if (newPlan.IsUpgradeFrom(CurrentPlan))
        {
            // Upgrades take effect immediately
            CurrentPlan = newPlan;
            PendingPlan = null;
            AddDomainEvent(new PlanChangedEvent(Id, oldPlan.Type, newPlan.Type, true, now));
        }
        else
        {
            // Downgrades take effect at end of billing period
            PendingPlan = newPlan;
            AddDomainEvent(new PlanChangedEvent(Id, oldPlan.Type, newPlan.Type, false, now));
        }

        return Result.Success();
    }

    public Result ApplyPendingPlanChange(DateTime now)
    {
        if (PendingPlan is null)
            return Result.Failure("No pending plan change");
        if (CurrentBillingPeriodEnd is null || now < CurrentBillingPeriodEnd.Value)
            return Result.Failure("Billing period has not ended yet");

        CurrentPlan = PendingPlan;
        PendingPlan = null;
        CurrentBillingPeriodEnd = now.AddMonths(1);
        return Result.Success();
    }

    public Result MarkPastDue(DateTime now)
    {
        if (Status != SubscriptionStatus.Active)
            return Result.Failure("Only active subscriptions can be marked past due");

        Status = SubscriptionStatus.PastDue;
        PastDueDate = now;

        AddDomainEvent(new SubscriptionPastDueEvent(Id, now));
        return Result.Success();
    }

    public Result AutoCancelIfPastDueExpired(DateTime now)
    {
        if (Status != SubscriptionStatus.PastDue)
            return Result.Failure("Subscription is not past due");
        if (PastDueDate is null)
            return Result.Failure("Past due date is not set");

        var daysPastDue = (now - PastDueDate.Value).TotalDays;
        if (daysPastDue < PastDueGracePeriodDays)
            return Result.Failure($"Grace period has not expired ({daysPastDue:F0} of {PastDueGracePeriodDays} days)");

        Status = SubscriptionStatus.Cancelled;
        CancelledDate = now;
        CoolingOffEndDate = null; // No cooling-off for auto-cancellation
        PendingPlan = null;

        AddDomainEvent(new SubscriptionCancelledEvent(Id, now, now, now));
        return Result.Success();
    }

    public bool IsTrialExpired(DateTime now) =>
        Status == SubscriptionStatus.Trial && TrialEndDate.HasValue && now >= TrialEndDate.Value;

    public bool IsCoolingOffExpired(DateTime now) =>
        Status == SubscriptionStatus.Cancelled && CoolingOffEndDate.HasValue && now > CoolingOffEndDate.Value;
}
