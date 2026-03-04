using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Entities;

public class ReservationResult
{
    public Quantity Reserved { get; }
    public Quantity Shortfall { get; }
    public Reservation? Reservation { get; }

    public ReservationResult(Quantity reserved, Quantity shortfall, Reservation? reservation)
    {
        Reserved = reserved;
        Shortfall = shortfall;
        Reservation = reservation;
    }

    /// <summary>Full reservation - everything requested was reserved.</summary>
    public bool IsFull => Shortfall.IsZero;

    /// <summary>Nothing was reserved (zero stock available).</summary>
    public bool IsEmpty => Reserved.IsZero;

    /// <summary>Some but not all was reserved.</summary>
    public bool IsPartial => !IsFull && !IsEmpty;
}
