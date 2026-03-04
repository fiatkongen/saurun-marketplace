using SubscriptionManagement.Domain;

namespace SubscriptionManagement.Services;

public sealed class SubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IDateTimeProvider _dateTime;

    public SubscriptionService(ISubscriptionRepository repository, IDateTimeProvider dateTime)
    {
        _repository = repository;
        _dateTime = dateTime;
    }

    public async Task<Result<Subscription>> StartTrialAsync(Guid customerId, PlanType planType, CancellationToken ct = default)
    {
        var existing = await _repository.GetByCustomerIdAsync(customerId, ct);
        if (existing is not null && existing.Status != SubscriptionStatus.Cancelled)
            return Result<Subscription>.Failure("Customer already has an active subscription.");

        var plan = Plan.FromType(planType);
        var subscription = Subscription.StartTrial(customerId, plan, _dateTime.UtcNow);
        await _repository.SaveAsync(subscription, ct);
        return Result<Subscription>.Success(subscription);
    }

    public async Task<Result<Subscription>> ChangePlanAsync(Guid subscriptionId, PlanType newPlanType, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result<Subscription>.Failure("Subscription not found.");

        var newPlan = Plan.FromType(newPlanType);
        var result = subscription.ChangePlan(newPlan, _dateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _repository.SaveAsync(subscription, ct);
        return result;
    }

    public async Task<Result<Subscription>> CancelAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result<Subscription>.Failure("Subscription not found.");

        var result = subscription.Cancel(_dateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _repository.SaveAsync(subscription, ct);
        return result;
    }

    public async Task<Result<Subscription>> ReactivateAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result<Subscription>.Failure("Subscription not found.");

        var result = subscription.Reactivate(_dateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _repository.SaveAsync(subscription, ct);
        return result;
    }

    public async Task<Result<Subscription>> MarkPastDueAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result<Subscription>.Failure("Subscription not found.");

        var result = subscription.MarkPastDue(_dateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _repository.SaveAsync(subscription, ct);
        return result;
    }

    public async Task<Result<Subscription>> ResolvePaymentAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result<Subscription>.Failure("Subscription not found.");

        var result = subscription.ResolvePayment(_dateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _repository.SaveAsync(subscription, ct);
        return result;
    }

    public async Task<Result<Subscription>> ApplyPendingDowngradeAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result<Subscription>.Failure("Subscription not found.");

        var result = subscription.ApplyPendingPlanChange(_dateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _repository.SaveAsync(subscription, ct);
        return result;
    }

    public async Task<int> ProcessExpiredTrialsAsync(CancellationToken ct = default)
    {
        var now = _dateTime.UtcNow;
        var expiredTrials = await _repository.GetExpiredTrialsAsync(now, ct);
        var activated = 0;

        foreach (var subscription in expiredTrials)
        {
            var result = subscription.Activate(now);
            if (result.IsSuccess)
            {
                await _repository.SaveAsync(subscription, ct);
                activated++;
            }
        }

        return activated;
    }

    public async Task<int> ProcessPastDueSubscriptionsAsync(CancellationToken ct = default)
    {
        var now = _dateTime.UtcNow;
        var pastDue = await _repository.GetPastDueSubscriptionsAsync(ct);
        var cancelled = 0;

        foreach (var subscription in pastDue)
        {
            var result = subscription.AutoCancelIfPastDueExpired(now);
            if (result.IsSuccess)
            {
                await _repository.SaveAsync(subscription, ct);
                cancelled++;
            }
        }

        return cancelled;
    }
}
