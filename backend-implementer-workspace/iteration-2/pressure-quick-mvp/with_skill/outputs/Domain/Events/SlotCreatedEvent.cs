using Domain.Common;

namespace Domain.Events;

public record SlotCreatedEvent(Guid SlotId, DateTime OccurredAt) : IDomainEvent;
