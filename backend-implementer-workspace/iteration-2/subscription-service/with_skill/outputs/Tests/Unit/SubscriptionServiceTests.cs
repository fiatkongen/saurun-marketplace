using NSubstitute;
using SubscriptionManagement.Domain.Entities;
using SubscriptionManagement.Domain.Interfaces;
using SubscriptionManagement.Domain.ValueObjects;
using SubscriptionManagement.Services;

namespace SubscriptionManagement.Tests.Unit;

public class SubscriptionServiceTests
{
    private readonly ISubscriptionRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _repository = Substitute.For<ISubscriptionRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        _sut = new SubscriptionService(_repository, _dateTimeProvider);
    }

    [Fact]
    public async Task StartTrial_WithValidCustomer_CreatesTrialSubscription()
    {
        var customerId = Guid.NewGuid();

        var result = await _sut.StartTrialAsync(customerId, Plan.Basic);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Trial, result.Value.Status);
        Assert.Equal(Plan.Basic, result.Value.Plan);
        await _repository.Received(1).AddAsync(result.Value, Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePlan_ExistingSubscription_DelegatesToEntity()
    {
        var subscriptionId = Guid.NewGuid();
        var now = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var sub = Subscription.Create(Guid.NewGuid(), Plan.Basic, now.AddDays(-20)).Value;
        sub.Activate(now.AddDays(-6));
        _repository.GetByIdAsync(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(sub);

        var result = await _sut.ChangePlanAsync(subscriptionId, Plan.Pro);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Pro, sub.Plan);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePlan_NonExistentSubscription_ReturnsFailure()
    {
        var subscriptionId = Guid.NewGuid();
        _repository.GetByIdAsync(subscriptionId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await _sut.ChangePlanAsync(subscriptionId, Plan.Pro);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CancelSubscription_ExistingActive_Cancels()
    {
        var subscriptionId = Guid.NewGuid();
        var now = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var sub = Subscription.Create(Guid.NewGuid(), Plan.Basic, now.AddDays(-20)).Value;
        sub.Activate(now.AddDays(-6));
        _repository.GetByIdAsync(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(sub);

        var result = await _sut.CancelAsync(subscriptionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateSubscription_WithinCoolingOff_Reactivates()
    {
        var subscriptionId = Guid.NewGuid();
        var now = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var sub = Subscription.Create(Guid.NewGuid(), Plan.Basic, now.AddDays(-30)).Value;
        sub.Activate(now.AddDays(-16));
        sub.Cancel(now.AddDays(-5)); // Cancelled 5 days ago, within 14-day window
        _repository.GetByIdAsync(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(sub);

        var result = await _sut.ReactivateAsync(subscriptionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPastDueAutoCancellations_CancelsPastDueSubscriptions()
    {
        var now = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var sub1 = Subscription.Create(Guid.NewGuid(), Plan.Basic, now.AddDays(-60)).Value;
        sub1.Activate(now.AddDays(-46));
        sub1.MarkPastDue(now.AddDays(-31)); // Past due 31 days ago, auto-cancel date passed

        _repository.GetPastDueForAutoCancellationAsync(now, Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub1 });

        var result = await _sut.ProcessPastDueAutoCancellationsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Equal(SubscriptionStatus.Cancelled, sub1.Status);
    }

    [Fact]
    public async Task ProcessTrialExpirations_ActivatesExpiredTrials()
    {
        var now = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(now);

        var sub1 = Subscription.Create(Guid.NewGuid(), Plan.Basic, now.AddDays(-15)).Value;
        // Trial ended yesterday

        _repository.GetExpiredTrialsAsync(now, Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub1 });

        var result = await _sut.ProcessTrialExpirationsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Equal(SubscriptionStatus.Active, sub1.Status);
    }
}
