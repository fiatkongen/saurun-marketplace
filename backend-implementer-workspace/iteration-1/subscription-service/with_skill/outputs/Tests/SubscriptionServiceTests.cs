using NSubstitute;
using SubscriptionManagement.Domain.Entities;
using SubscriptionManagement.Domain.Enums;
using SubscriptionManagement.Domain.Interfaces;
using SubscriptionManagement.Domain.ValueObjects;
using SubscriptionManagement.Services;
using Xunit;

namespace SubscriptionManagement.Tests;

public class SubscriptionServiceTests
{
    private static readonly DateTime Now = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly ISubscriptionRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _repository = Substitute.For<ISubscriptionRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(Now);
        _sut = new SubscriptionService(_repository, _dateTimeProvider);
    }

    // ============================================================
    // StartTrialAsync
    // ============================================================

    [Fact]
    public async Task StartTrialAsync_WithValidInputs_ReturnsSuccessAndPersists()
    {
        var customerId = Guid.NewGuid();

        var result = await _sut.StartTrialAsync(customerId, PlanType.Pro);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Trial, result.Value.Status);
        Assert.Equal(PlanType.Pro, result.Value.CurrentPlan.Type);
        await _repository.Received(1).AddAsync(result.Value, Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartTrialAsync_WithEmptyCustomerId_ReturnsFailure()
    {
        var result = await _sut.StartTrialAsync(Guid.Empty, PlanType.Basic);

        Assert.True(result.IsFailure);
        Assert.Equal("Customer ID is required", result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(PlanType.Basic)]
    [InlineData(PlanType.Pro)]
    [InlineData(PlanType.Enterprise)]
    public async Task StartTrialAsync_AllPlanTypes_CreateWithCorrectPlan(PlanType planType)
    {
        var result = await _sut.StartTrialAsync(Guid.NewGuid(), planType);

        Assert.True(result.IsSuccess);
        Assert.Equal(planType, result.Value.CurrentPlan.Type);
    }

    // ============================================================
    // ActivateTrialAsync
    // ============================================================

    [Fact]
    public async Task ActivateTrialAsync_ExistingTrialSubscription_ActivatesAndSaves()
    {
        var sub = CreateTrialSubscription();
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ActivateTrialAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateTrialAsync_SubscriptionNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var result = await _sut.ActivateTrialAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription not found", result.Error);
    }

    [Fact]
    public async Task ActivateTrialAsync_AlreadyActive_ReturnsFailure()
    {
        var sub = CreateActiveSubscription();
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ActivateTrialAsync(sub.Id);

        Assert.True(result.IsFailure);
    }

    // ============================================================
    // ChangePlanAsync
    // ============================================================

    [Fact]
    public async Task ChangePlanAsync_UpgradeOnActiveSubscription_Succeeds()
    {
        var sub = CreateActiveSubscription(PlanType.Basic);
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ChangePlanAsync(sub.Id, PlanType.Pro);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Pro, sub.CurrentPlan.Type);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePlanAsync_DowngradeOnActiveSubscription_SetsPending()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ChangePlanAsync(sub.Id, PlanType.Basic);

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Pro, sub.CurrentPlan.Type); // unchanged
        Assert.Equal(PlanType.Basic, sub.PendingPlan!.Type);
    }

    [Fact]
    public async Task ChangePlanAsync_SubscriptionNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var result = await _sut.ChangePlanAsync(Guid.NewGuid(), PlanType.Pro);

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription not found", result.Error);
    }

    [Fact]
    public async Task ChangePlanAsync_DowngradeDuringTrial_ReturnsFailure()
    {
        var sub = CreateTrialSubscription(PlanType.Pro);
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ChangePlanAsync(sub.Id, PlanType.Basic);

        Assert.True(result.IsFailure);
        Assert.Equal("Cannot downgrade during trial period", result.Error);
    }

    // ============================================================
    // CancelAsync
    // ============================================================

    [Fact]
    public async Task CancelAsync_ActiveSubscription_CancelsAndSaves()
    {
        var sub = CreateActiveSubscription();
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.CancelAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_SubscriptionNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var result = await _sut.CancelAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription not found", result.Error);
    }

    // ============================================================
    // ReactivateAsync
    // ============================================================

    [Fact]
    public async Task ReactivateAsync_WithinCoolingOff_ReactivatesAndSaves()
    {
        var sub = CreateCancelledSubscription();
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(5)); // within cooling-off

        var result = await _sut.ReactivateAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_AfterCoolingOff_ReturnsFailure()
    {
        var sub = CreateCancelledSubscription();
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(15)); // past cooling-off

        var result = await _sut.ReactivateAsync(sub.Id);

        Assert.True(result.IsFailure);
        Assert.Equal("Cooling-off period has expired", result.Error);
    }

    [Fact]
    public async Task ReactivateAsync_SubscriptionNotFound_ReturnsFailure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var result = await _sut.ReactivateAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("Subscription not found", result.Error);
    }

    // ============================================================
    // ProcessExpiredTrialsAsync
    // ============================================================

    [Fact]
    public async Task ProcessExpiredTrialsAsync_ExpiredTrial_ActivatesIt()
    {
        var sub = CreateTrialSubscription();
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(15)); // past 14-day trial

        var result = await _sut.ProcessExpiredTrialsAsync(new[] { sub });

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessExpiredTrialsAsync_ActiveTrial_LeavesAsIs()
    {
        var sub = CreateTrialSubscription();
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(5)); // within trial

        await _sut.ProcessExpiredTrialsAsync(new[] { sub });

        Assert.Equal(SubscriptionStatus.Trial, sub.Status);
    }

    // ============================================================
    // ProcessPastDueSubscriptionsAsync
    // ============================================================

    [Fact]
    public async Task ProcessPastDueSubscriptionsAsync_After30Days_AutoCancels()
    {
        var sub = CreatePastDueSubscription();
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(31));

        var result = await _sut.ProcessPastDueSubscriptionsAsync(new[] { sub });

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public async Task ProcessPastDueSubscriptionsAsync_Before30Days_StaysPastDue()
    {
        var sub = CreatePastDueSubscription();
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(20));

        await _sut.ProcessPastDueSubscriptionsAsync(new[] { sub });

        Assert.Equal(SubscriptionStatus.PastDue, sub.Status);
    }

    // ============================================================
    // ApplyPendingDowngradesAsync
    // ============================================================

    [Fact]
    public async Task ApplyPendingDowngradesAsync_AtBillingEnd_AppliesDowngrade()
    {
        var sub = CreateActiveSubscription(PlanType.Pro);
        sub.ChangePlan(Plan.Basic, Now);
        var billingEnd = sub.CurrentBillingPeriodEnd!.Value;
        _dateTimeProvider.UtcNow.Returns(billingEnd);

        var result = await _sut.ApplyPendingDowngradesAsync(new[] { sub });

        Assert.True(result.IsSuccess);
        Assert.Equal(PlanType.Basic, sub.CurrentPlan.Type);
        Assert.Null(sub.PendingPlan);
    }

    [Fact]
    public async Task ApplyPendingDowngradesAsync_BeforeBillingEnd_KeepsPending()
    {
        var sub = CreateActiveSubscription(PlanType.Enterprise);
        sub.ChangePlan(Plan.Basic, Now);
        _dateTimeProvider.UtcNow.Returns(Now.AddDays(5));

        await _sut.ApplyPendingDowngradesAsync(new[] { sub });

        Assert.Equal(PlanType.Enterprise, sub.CurrentPlan.Type);
        Assert.NotNull(sub.PendingPlan);
    }

    // ============================================================
    // Helpers - Use real domain objects, never mocked
    // ============================================================

    private static Subscription CreateTrialSubscription(PlanType planType = PlanType.Basic)
    {
        var plan = Plan.FromType(planType).Value;
        return Subscription.CreateTrial(Guid.NewGuid(), plan, Now).Value;
    }

    private static Subscription CreateActiveSubscription(PlanType planType = PlanType.Basic)
    {
        var sub = CreateTrialSubscription(planType);
        sub.Activate(Now);
        return sub;
    }

    private static Subscription CreateCancelledSubscription(PlanType planType = PlanType.Basic)
    {
        var sub = CreateActiveSubscription(planType);
        sub.Cancel(Now);
        return sub;
    }

    private static Subscription CreatePastDueSubscription(PlanType planType = PlanType.Basic)
    {
        var sub = CreateActiveSubscription(planType);
        sub.MarkPastDue(Now);
        return sub;
    }
}
