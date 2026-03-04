using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Entities;
using SubscriptionManagement.Domain.Enums;
using SubscriptionManagement.Domain.Interfaces;
using SubscriptionManagement.Domain.ValueObjects;

namespace SubscriptionManagement.Services;

public class SubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubscriptionService(ISubscriptionRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Subscription>> StartTrialAsync(
        Guid customerId, PlanType planType, CancellationToken ct = default)
    {
        var planResult = Plan.FromType(planType);
        if (planResult.IsFailure)
            return Result.Failure<Subscription>(planResult.Error);

        var now = _dateTimeProvider.UtcNow;
        var subscriptionResult = Subscription.CreateTrial(customerId, planResult.Value, now);
        if (subscriptionResult.IsFailure)
            return subscriptionResult;

        await _repository.AddAsync(subscriptionResult.Value, ct);
        await _repository.SaveChangesAsync(ct);
        return subscriptionResult;
    }

    public async Task<Result> ActivateTrialAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result.Failure("Subscription not found");

        var now = _dateTimeProvider.UtcNow;
        var result = subscription.Activate(now);
        if (result.IsFailure)
            return result;

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ChangePlanAsync(
        Guid subscriptionId, PlanType newPlanType, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result.Failure("Subscription not found");

        var planResult = Plan.FromType(newPlanType);
        if (planResult.IsFailure)
            return Result.Failure(planResult.Error);

        var now = _dateTimeProvider.UtcNow;
        var result = subscription.ChangePlan(planResult.Value, now);
        if (result.IsFailure)
            return result;

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result.Failure("Subscription not found");

        var now = _dateTimeProvider.UtcNow;
        var result = subscription.Cancel(now);
        if (result.IsFailure)
            return result;

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ReactivateAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result.Failure("Subscription not found");

        var now = _dateTimeProvider.UtcNow;
        var result = subscription.Reactivate(now);
        if (result.IsFailure)
            return result;

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ProcessExpiredTrialsAsync(
        IEnumerable<Subscription> trialSubscriptions, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.UtcNow;

        foreach (var subscription in trialSubscriptions)
        {
            if (subscription.IsTrialExpired(now))
            {
                var result = subscription.Activate(now);
                if (result.IsFailure)
                    continue; // Log in production
            }
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ProcessPastDueSubscriptionsAsync(
        IEnumerable<Subscription> pastDueSubscriptions, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.UtcNow;

        foreach (var subscription in pastDueSubscriptions)
        {
            subscription.AutoCancelIfPastDueExpired(now);
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ApplyPendingDowngradesAsync(
        IEnumerable<Subscription> subscriptionsWithPendingChanges, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.UtcNow;

        foreach (var subscription in subscriptionsWithPendingChanges)
        {
            subscription.ApplyPendingPlanChange(now);
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
