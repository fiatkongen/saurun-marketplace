using Domain.Base;

namespace Domain.Events;

public record OrderConfirmedEvent(
    Guid OrderId,
    DateTime OccurredAt
) : IDomainEvent;
