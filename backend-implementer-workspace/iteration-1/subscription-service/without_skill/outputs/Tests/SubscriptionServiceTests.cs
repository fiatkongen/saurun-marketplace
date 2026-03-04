using NSubstitute;
using SubscriptionManagement.Domain;
using SubscriptionManagement.Services;
using Xunit;

namespace SubscriptionManagement.Tests;

public class SubscriptionServiceTests
{
    private readonly ISubscriptionRepository _repository;
    private readonly IDateTimeProvider _dateTime;
    private readonly SubscriptionService _sut;
    private static readonly DateTime Now = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    public SubscriptionServiceTests()
    {
        _repository = Substitute.For<ISubscriptionRepository>();
        _dateTime = Substitute.For<IDateTimeProvider>();
        _dateTime.UtcNow.Returns(Now);
        _sut = new SubscriptionService(_repository, _dateTime);
    }

    // --- StartTrialAsync ---

    [Fact]
    public async Task StartTrialAsync_Creates_Trial_Subscription()
    {
        var customerId = Guid.NewGuid();
        _repository.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await _sut.StartTrialAsync(customerId, PlanType.Pro);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Trial, result.Value.Status);
        Assert.Equal(Plan.Pro, result.Value.CurrentPlan);
        await _repository.Received(1).SaveAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartTrialAsync_Fails_If_Customer_Has_Active_Subscription()
    {
        var customerId = Guid.NewGuid();
        var existing = Subscription.StartTrial(customerId, Plan.Basic, Now);
        _repository.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.StartTrialAsync(customerId, PlanType.Pro);

        Assert.True(result.IsFailure);
        Assert.Contains("already has an active subscription", result.Error);
        await _repository.DidNotReceive().SaveAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartTrialAsync_Allows_If_Customer_Has_Cancelled_Subscription()
    {
        var customerId = Guid.NewGuid();
        var existing = Subscription.StartTrial(customerId, Plan.Basic, Now);
        existing.Activate(Now.AddDays(14));
        existing.Cancel(Now.AddDays(20));
        _repository.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await _sut.StartTrialAsync(customerId, PlanType.Pro);

        Assert.True(result.IsSuccess);
    }

    // --- ChangePlanAsync ---

    [Fact]
    public async Task ChangePlanAsync_Returns_Failure_If_Not_Found()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await _sut.ChangePlanAsync(Guid.NewGuid(), PlanType.Pro);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task ChangePlanAsync_Upgrade_Saves_Immediately()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        _dateTime.UtcNow.Returns(Now.AddDays(20));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ChangePlanAsync(sub.Id, PlanType.Enterprise);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, result.Value.CurrentPlan);
        await _repository.Received(1).SaveAsync(sub, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePlanAsync_Downgrade_During_Trial_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Pro, Now);
        _dateTime.UtcNow.Returns(Now.AddDays(3));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ChangePlanAsync(sub.Id, PlanType.Basic);

        Assert.True(result.IsFailure);
        await _repository.DidNotReceive().SaveAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePlanAsync_Downgrade_Schedules_For_End_Of_Billing_Period()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Activate(Now.AddDays(14));
        _dateTime.UtcNow.Returns(Now.AddDays(20));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ChangePlanAsync(sub.Id, PlanType.Basic);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Enterprise, result.Value.CurrentPlan); // Still on Enterprise
        Assert.Equal(Plan.Basic, result.Value.PendingPlan);
        await _repository.Received(1).SaveAsync(sub, Arg.Any<CancellationToken>());
    }

    // --- CancelAsync ---

    [Fact]
    public async Task CancelAsync_Sets_CoolingOff()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        var cancelTime = Now.AddDays(20);
        _dateTime.UtcNow.Returns(cancelTime);
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.CancelAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, result.Value.Status);
        Assert.Equal(cancelTime.AddDays(14), result.Value.CancellationEffectiveAtUtc);
    }

    [Fact]
    public async Task CancelAsync_Not_Found_Returns_Failure()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await _sut.CancelAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
    }

    // --- ReactivateAsync ---

    [Fact]
    public async Task ReactivateAsync_During_CoolingOff_Succeeds()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(20));
        _dateTime.UtcNow.Returns(Now.AddDays(25));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ReactivateAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, result.Value.Status);
        await _repository.Received(1).SaveAsync(sub, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateAsync_After_CoolingOff_Fails()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.Cancel(Now.AddDays(20));
        _dateTime.UtcNow.Returns(Now.AddDays(40));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ReactivateAsync(sub.Id);

        Assert.True(result.IsFailure);
        await _repository.DidNotReceive().SaveAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    // --- ProcessExpiredTrialsAsync ---

    [Fact]
    public async Task ProcessExpiredTrialsAsync_Activates_Expired_Trials()
    {
        var sub1 = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now.AddDays(-15));
        var sub2 = Subscription.StartTrial(Guid.NewGuid(), Plan.Pro, Now.AddDays(-16));
        _dateTime.UtcNow.Returns(Now);
        _repository.GetExpiredTrialsAsync(Now, Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub1, sub2 });

        var count = await _sut.ProcessExpiredTrialsAsync();

        Assert.Equal(2, count);
        await _repository.Received(2).SaveAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    // --- ProcessPastDueSubscriptionsAsync ---

    [Fact]
    public async Task ProcessPastDueSubscriptionsAsync_Cancels_Expired_PastDue()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now.AddDays(-60));
        sub.Activate(Now.AddDays(-46));
        sub.MarkPastDue(Now.AddDays(-31));
        _dateTime.UtcNow.Returns(Now);
        _repository.GetPastDueSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub });

        var count = await _sut.ProcessPastDueSubscriptionsAsync();

        Assert.Equal(1, count);
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public async Task ProcessPastDueSubscriptionsAsync_Does_Not_Cancel_Within_Grace_Period()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now.AddDays(-30));
        sub.Activate(Now.AddDays(-16));
        sub.MarkPastDue(Now.AddDays(-10));
        _dateTime.UtcNow.Returns(Now);
        _repository.GetPastDueSubscriptionsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub });

        var count = await _sut.ProcessPastDueSubscriptionsAsync();

        Assert.Equal(0, count);
        Assert.Equal(SubscriptionStatus.PastDue, sub.Status);
    }

    // --- MarkPastDueAsync ---

    [Fact]
    public async Task MarkPastDueAsync_Succeeds_For_Active()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        _dateTime.UtcNow.Returns(Now.AddDays(45));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.MarkPastDueAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.PastDue, result.Value.Status);
    }

    // --- ResolvePaymentAsync ---

    [Fact]
    public async Task ResolvePaymentAsync_Returns_To_Active()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Basic, Now);
        sub.Activate(Now.AddDays(14));
        sub.MarkPastDue(Now.AddDays(45));
        _dateTime.UtcNow.Returns(Now.AddDays(50));
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ResolvePaymentAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, result.Value.Status);
    }

    // --- ApplyPendingDowngradeAsync ---

    [Fact]
    public async Task ApplyPendingDowngradeAsync_Applies_When_Due()
    {
        var sub = Subscription.StartTrial(Guid.NewGuid(), Plan.Enterprise, Now);
        sub.Activate(Now.AddDays(14));
        sub.ChangePlan(Plan.Basic, Now.AddDays(20));
        var effectiveDate = sub.PendingPlanEffectiveDate!.Value;
        _dateTime.UtcNow.Returns(effectiveDate);
        _repository.GetByIdAsync(sub.Id, Arg.Any<CancellationToken>()).Returns(sub);

        var result = await _sut.ApplyPendingDowngradeAsync(sub.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(Plan.Basic, result.Value.CurrentPlan);
        Assert.Null(result.Value.PendingPlan);
    }
}
