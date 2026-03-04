using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionReactivatedEvent(
    Guid SubscriptionId,
    DateTime OccurredAt
) : IDomainEvent;
