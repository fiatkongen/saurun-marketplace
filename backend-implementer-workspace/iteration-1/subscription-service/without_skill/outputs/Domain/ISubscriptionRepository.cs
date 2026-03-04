namespace SubscriptionManagement.Domain;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Subscription?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Subscription>> GetPastDueSubscriptionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Subscription>> GetExpiredTrialsAsync(DateTime asOfUtc, CancellationToken ct = default);
    Task SaveAsync(Subscription subscription, CancellationToken ct = default);
}
