using Domain.Common;

namespace Domain.Events;

public record BookingCreatedEvent(
    Guid BookingId,
    Guid SlotId,
    Guid CustomerId,
    DateTime OccurredAt
) : IDomainEvent;
