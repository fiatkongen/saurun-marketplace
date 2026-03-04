using Domain.Common;

namespace Domain.Events;

public record ProductUpdatedEvent(
    Guid ProductId,
    DateTime OccurredAt
) : IDomainEvent;
