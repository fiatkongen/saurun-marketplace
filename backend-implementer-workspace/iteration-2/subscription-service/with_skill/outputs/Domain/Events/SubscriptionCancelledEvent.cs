using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionCancelledEvent(
    Guid SubscriptionId,
    Guid CustomerId,
    DateTime OccurredAt
) : IDomainEvent;
