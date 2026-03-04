using InventoryReservation.Domain.Entities;

namespace InventoryReservation.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a reservation request, including shortfall information
/// for partial reservations.
/// </summary>
public class ReservationOutcome
{
    public Reservation Reservation { get; }
    public int Shortfall { get; }
    public bool IsFullyReserved => Shortfall == 0;

    private ReservationOutcome(Reservation reservation, int shortfall)
    {
        Reservation = reservation;
        Shortfall = shortfall;
    }

    public static ReservationOutcome FullyReserved(Reservation reservation) =>
        new(reservation, 0);

    public static ReservationOutcome PartiallyReserved(Reservation reservation, int shortfall) =>
        new(reservation, shortfall);
}
