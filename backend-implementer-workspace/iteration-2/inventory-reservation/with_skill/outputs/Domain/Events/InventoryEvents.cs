using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.Events;

public record StockReservedEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    Guid ProductId,
    int Quantity,
    int Shortfall,
    DateTime OccurredAt
) : IDomainEvent;

public record ReservationConfirmedEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    Guid ProductId,
    int Quantity,
    DateTime OccurredAt
) : IDomainEvent;

public record ReservationCancelledEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    Guid ProductId,
    int Quantity,
    DateTime OccurredAt
) : IDomainEvent;

public record ReservationExpiredEvent(
    Guid InventoryItemId,
    Guid ReservationId,
    Guid ProductId,
    int Quantity,
    DateTime OccurredAt
) : IDomainEvent;

public record InventoryItemCreatedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int InitialQuantity,
    DateTime OccurredAt
) : IDomainEvent;
