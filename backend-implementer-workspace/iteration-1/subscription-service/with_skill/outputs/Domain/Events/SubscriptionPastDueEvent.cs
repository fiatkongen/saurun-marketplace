using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionPastDueEvent(
    Guid SubscriptionId,
    DateTime OccurredAt
) : IDomainEvent;
