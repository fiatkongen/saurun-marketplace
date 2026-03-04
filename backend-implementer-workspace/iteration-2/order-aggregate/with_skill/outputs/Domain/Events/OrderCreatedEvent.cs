using Domain.Base;

namespace Domain.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    DateTime OccurredAt
) : IDomainEvent;
