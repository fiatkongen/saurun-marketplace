using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Events;
using SubscriptionManagement.Domain.ValueObjects;

namespace SubscriptionManagement.Domain.Entities;

public class Subscription : AggregateRoot<Guid>
{
    private const int TrialDurationDays = 14;
    private const int CoolingOffDays = 14;
    private const int PastDueAutoCancelDays = 30;

    public Guid CustomerId { get; private set; }
    public Plan Plan { get; private set; } = null!;
    public Plan? PendingPlan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime TrialEndDate { get; private set; }
    public DateTime? CancellationDate { get; private set; }
    public DateTime? CancellationCoolingOffEndDate { get; private set; }
    public DateTime? AutoCancelDate { get; private set; }
    public DateTime? BillingPeriodEndDate { get; private set; }

    private Subscription() { } // EF Core

    private Subscription(Guid id, Guid customerId, Plan plan, DateTime now) : base(id)
    {
        CustomerId = customerId;
        Plan = plan;
        Status = SubscriptionStatus.Trial;
        CreatedAt = now;
        TrialEndDate = now.AddDays(TrialDurationDays);
    }

    public static Result<Subscription> Create(Guid customerId, Plan plan, DateTime now)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Subscription>("Customer ID is required");

        var subscription = new Subscription(Guid.NewGuid(), customerId, plan, now);
        subscription.AddDomainEvent(new SubscriptionCreatedEvent(
            subscription.Id, customerId, plan.Name, now));
        return Result.Success(subscription);
    }

    public Result Activate(DateTime now)
    {
        if (Status == SubscriptionStatus.Active)
            return Result.Failure("Subscription is already active");
        if (Status != SubscriptionStatus.Trial)
            return Result.Failure("Only trial subscriptions can be activated");

        Status = SubscriptionStatus.Active;
        BillingPeriodEndDate = now.AddDays(30);
        AddDomainEvent(new SubscriptionActivatedEvent(Id, now));
        return Result.Success();
    }

    public Result Cancel(DateTime now)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result.Failure("Subscription is already cancelled");

        CancellationDate = now;
        CancellationCoolingOffEndDate = now.AddDays(CoolingOffDays);
        Status = SubscriptionStatus.Cancelled;
        AddDomainEvent(new SubscriptionCancelledEvent(Id, CustomerId, now));
        return Result.Success();
    }

    public Result Reactivate(DateTime now)
    {
        if (Status != SubscriptionStatus.Cancelled)
            return Result.Failure("Only cancelled subscriptions can be reactivated");

        if (CancellationCoolingOffEndDate.HasValue && now > CancellationCoolingOffEndDate.Value)
            return Result.Failure("Cannot reactivate after cooling-off period has expired");

        Status = SubscriptionStatus.Active;
        CancellationDate = null;
        CancellationCoolingOffEndDate = null;
        AddDomainEvent(new SubscriptionReactivatedEvent(Id, now));
        return Result.Success();
    }

    public Result MarkPastDue(DateTime now)
    {
        if (Status != SubscriptionStatus.Active)
            return Result.Failure("Only active subscriptions can be marked past due");

        Status = SubscriptionStatus.PastDue;
        AutoCancelDate = now.AddDays(PastDueAutoCancelDays);
        return Result.Success();
    }

    public Result ChangePlan(Plan newPlan, DateTime now)
    {
        if (Status == SubscriptionStatus.Cancelled)
            return Result.Failure("Cannot change plan on a cancelled subscription");

        if (Status == SubscriptionStatus.PastDue)
            return Result.Failure("Cannot change plan on a past due subscription");

        if (Plan == newPlan)
            return Result.Failure("Cannot change to the same plan");

        // Cannot downgrade during trial
        if (Status == SubscriptionStatus.Trial && newPlan.IsDowngradeFrom(Plan))
            return Result.Failure("Cannot downgrade during trial period");

        var previousPlan = Plan;

        if (newPlan.IsUpgradeFrom(Plan))
        {
            // Upgrades take effect immediately
            Plan = newPlan;
            PendingPlan = null;
            AddDomainEvent(new PlanChangedEvent(Id, previousPlan.Name, newPlan.Name, true, now));
        }
        else
        {
            // Downgrades at end of billing period
            PendingPlan = newPlan;
            AddDomainEvent(new PlanChangedEvent(Id, previousPlan.Name, newPlan.Name, false, now));
        }

        return Result.Success();
    }

    public Result ApplyPendingPlanChange()
    {
        if (PendingPlan is null)
            return Result.Failure("No pending plan change to apply");

        Plan = PendingPlan;
        PendingPlan = null;
        return Result.Success();
    }
}
