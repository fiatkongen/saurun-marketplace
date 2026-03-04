using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.Events;

public record InventoryItemCreatedEvent(
    Guid InventoryItemId,
    Guid ProductId,
    int InitialQuantity,
    DateTime OccurredAt
) : IDomainEvent;
