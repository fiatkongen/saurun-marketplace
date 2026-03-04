using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record SubscriptionCancelledEvent(
    Guid SubscriptionId,
    DateTime CancellationDate,
    DateTime CoolingOffEndDate,
    DateTime OccurredAt
) : IDomainEvent;
