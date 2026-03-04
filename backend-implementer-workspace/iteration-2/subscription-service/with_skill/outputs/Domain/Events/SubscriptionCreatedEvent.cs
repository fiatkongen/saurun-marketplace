using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionCreatedEvent(
    Guid SubscriptionId,
    Guid CustomerId,
    string PlanName,
    DateTime OccurredAt
) : IDomainEvent;
