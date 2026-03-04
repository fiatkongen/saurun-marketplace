using Domain.Common;

namespace Domain.Events;

public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    DateTime OccurredAt
) : IDomainEvent;
