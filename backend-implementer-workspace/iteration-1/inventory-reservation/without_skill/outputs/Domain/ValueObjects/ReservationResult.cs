namespace InventoryReservation.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a reservation attempt, including partial fulfillment info.
/// </summary>
public sealed record ReservationResult
{
    public ReservationId ReservationId { get; }
    public Quantity ReservedQuantity { get; }
    public Quantity RequestedQuantity { get; }
    public Quantity Shortfall { get; }
    public bool IsPartial => Shortfall > Quantity.Zero;

    public ReservationResult(
        ReservationId reservationId,
        Quantity reservedQuantity,
        Quantity requestedQuantity)
    {
        ReservationId = reservationId;
        ReservedQuantity = reservedQuantity;
        RequestedQuantity = requestedQuantity;
        Shortfall = requestedQuantity - reservedQuantity;
    }
}
