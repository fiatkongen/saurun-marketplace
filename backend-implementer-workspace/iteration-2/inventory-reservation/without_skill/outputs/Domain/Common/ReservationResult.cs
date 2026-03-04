using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Common;

/// <summary>
/// Represents the outcome of a reservation attempt, including partial fills.
/// </summary>
public sealed record ReservationResult
{
    public ReservationId ReservationId { get; }
    public Quantity ReservedQuantity { get; }
    public Quantity Shortfall { get; }
    public bool IsPartial => Shortfall.Value > 0;

    public ReservationResult(ReservationId reservationId, Quantity reserved, Quantity shortfall)
    {
        ReservationId = reservationId;
        ReservedQuantity = reserved;
        Shortfall = shortfall;
    }
}
