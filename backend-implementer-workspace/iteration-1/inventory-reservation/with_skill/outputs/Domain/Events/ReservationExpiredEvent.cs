using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.Events;

public record ReservationExpiredEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    int QuantityReleased,
    DateTime OccurredAt
) : IDomainEvent;
