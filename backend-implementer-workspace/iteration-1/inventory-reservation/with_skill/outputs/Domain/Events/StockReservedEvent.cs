using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.Events;

public record StockReservedEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    int QuantityReserved,
    int Shortfall,
    DateTime OccurredAt
) : IDomainEvent;
