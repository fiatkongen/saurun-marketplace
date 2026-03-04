using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionActivatedEvent(
    Guid SubscriptionId,
    DateTime OccurredAt
) : IDomainEvent;
