using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Enums;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionCreatedEvent(
    Guid SubscriptionId,
    Guid CustomerId,
    PlanType Plan,
    DateTime OccurredAt
) : IDomainEvent;
