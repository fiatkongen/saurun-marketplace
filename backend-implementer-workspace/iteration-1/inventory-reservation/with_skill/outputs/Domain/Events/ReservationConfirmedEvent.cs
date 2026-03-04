using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.Events;

public record ReservationConfirmedEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    int QuantityConfirmed,
    DateTime OccurredAt
) : IDomainEvent;
