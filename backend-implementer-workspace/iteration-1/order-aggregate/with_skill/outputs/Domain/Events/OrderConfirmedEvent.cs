namespace OrderAggregate.Domain.Events;

public record OrderConfirmedEvent(
    Guid OrderId,
    DateTime OccurredAt
) : IDomainEvent;
