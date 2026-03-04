using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Entities;
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
        Guid customerId, Plan plan, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var result = Subscription.Create(customerId, plan, now);
        if (result.IsFailure)
            return result;

        await _repository.AddAsync(result.Value, ct);
        await _repository.SaveChangesAsync(ct);
        return result;
    }

    public async Task<Result> ChangePlanAsync(
        Guid subscriptionId, Plan newPlan, CancellationToken ct = default)
    {
        var subscription = await _repository.GetByIdAsync(subscriptionId, ct);
        if (subscription is null)
            return Result.Failure("Subscription not found");

        var now = _dateTimeProvider.UtcNow;
        var result = subscription.ChangePlan(newPlan, now);
        if (result.IsFailure)
            return result;

        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> CancelAsync(
        Guid subscriptionId, CancellationToken ct = default)
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

    public async Task<Result> ReactivateAsync(
        Guid subscriptionId, CancellationToken ct = default)
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

    public async Task<Result<int>> ProcessPastDueAutoCancellationsAsync(
        CancellationToken ct = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var subscriptions = await _repository.GetPastDueForAutoCancellationAsync(now, ct);

        var cancelledCount = 0;
        foreach (var subscription in subscriptions)
        {
            var result = subscription.Cancel(now);
            if (result.IsSuccess)
                cancelledCount++;
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success(cancelledCount);
    }

    public async Task<Result<int>> ProcessTrialExpirationsAsync(
        CancellationToken ct = default)
    {
        var now = _dateTimeProvider.UtcNow;
        var subscriptions = await _repository.GetExpiredTrialsAsync(now, ct);

        var activatedCount = 0;
        foreach (var subscription in subscriptions)
        {
            var result = subscription.Activate(now);
            if (result.IsSuccess)
                activatedCount++;
        }

        await _repository.SaveChangesAsync(ct);
        return Result.Success(activatedCount);
    }
}
