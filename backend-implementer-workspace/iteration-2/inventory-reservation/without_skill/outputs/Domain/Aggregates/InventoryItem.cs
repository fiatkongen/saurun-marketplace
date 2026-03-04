using InventoryReservation.Domain.Common;
using InventoryReservation.Domain.Entities;
using InventoryReservation.Domain.Enums;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Aggregates;

public class InventoryItem : AggregateRoot<ProductId>
{
    private readonly List<Reservation> _reservations = new();

    public Quantity AvailableQuantity { get; private set; } = Quantity.Zero;
    public Quantity ReservedQuantity { get; private set; } = Quantity.Zero;
    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

    private InventoryItem() { } // EF/serialization

    public InventoryItem(ProductId productId, Quantity initialStock)
    {
        Id = productId;
        AvailableQuantity = initialStock;
        ReservedQuantity = Quantity.Zero;
    }

    /// <summary>
    /// Reserves up to the requested quantity. Supports partial reservation:
    /// reserves whatever is available and reports any shortfall.
    /// </summary>
    public Result<ReservationResult> Reserve(
        Quantity requestedQuantity,
        DateTime utcNow,
        TimeSpan? expirationWindow = null)
    {
        if (requestedQuantity.Value <= 0)
            return Result.Failure<ReservationResult>("Requested quantity must be greater than zero.");

        // Expire any stale pending reservations first
        ExpireStaleReservations(utcNow);

        if (AvailableQuantity.Value == 0)
            return Result.Failure<ReservationResult>(
                "No stock available for reservation.");

        var expiration = expirationWindow ?? TimeSpan.FromMinutes(15);
        var quantityToReserve = requestedQuantity.Min(AvailableQuantity);
        var shortfall = Quantity.Of(requestedQuantity.Value - quantityToReserve.Value);

        var reservationId = ReservationId.Create();
        var reservation = new Reservation(
            reservationId,
            Id,
            quantityToReserve,
            utcNow.Add(expiration));

        _reservations.Add(reservation);
        AvailableQuantity -= quantityToReserve;
        ReservedQuantity += quantityToReserve;
        IncrementVersion();

        var result = new ReservationResult(reservationId, quantityToReserve, shortfall);
        return Result.Success(result);
    }

    /// <summary>
    /// Confirms a pending reservation. Decreases reserved stock permanently.
    /// The item is considered "sold" -- available stock is not changed (already removed on reserve).
    /// </summary>
    public Result ConfirmReservation(ReservationId reservationId, DateTime utcNow)
    {
        var reservation = FindReservation(reservationId);
        if (reservation is null)
            return Result.Failure($"Reservation '{reservationId}' not found.");

        // Check expiration before confirming
        if (reservation.IsExpired(utcNow))
        {
            ExpireSingleReservation(reservation);
            return Result.Failure($"Reservation '{reservationId}' has expired.");
        }

        if (!reservation.IsPending)
            return Result.Failure(
                $"Reservation '{reservationId}' is in '{reservation.Status}' status and cannot be confirmed.");

        reservation.Confirm();
        ReservedQuantity -= reservation.Quantity;
        IncrementVersion();

        return Result.Success();
    }

    /// <summary>
    /// Cancels a pending reservation. Returns reserved stock to available.
    /// </summary>
    public Result CancelReservation(ReservationId reservationId, DateTime utcNow)
    {
        var reservation = FindReservation(reservationId);
        if (reservation is null)
            return Result.Failure($"Reservation '{reservationId}' not found.");

        if (reservation.IsExpired(utcNow))
        {
            ExpireSingleReservation(reservation);
            return Result.Failure($"Reservation '{reservationId}' has already expired.");
        }

        if (!reservation.IsPending)
            return Result.Failure(
                $"Reservation '{reservationId}' is in '{reservation.Status}' status and cannot be cancelled.");

        reservation.Cancel();
        AvailableQuantity += reservation.Quantity;
        ReservedQuantity -= reservation.Quantity;
        IncrementVersion();

        return Result.Success();
    }

    /// <summary>
    /// Expires all stale pending reservations, returning their stock to available.
    /// </summary>
    public int ExpireStaleReservations(DateTime utcNow)
    {
        var stale = _reservations
            .Where(r => r.IsExpired(utcNow))
            .ToList();

        foreach (var reservation in stale)
        {
            ExpireSingleReservation(reservation);
        }

        return stale.Count;
    }

    /// <summary>
    /// Adds stock to the inventory item.
    /// </summary>
    public void Restock(Quantity quantity)
    {
        if (quantity.Value <= 0)
            throw new ArgumentException("Restock quantity must be positive.", nameof(quantity));

        AvailableQuantity += quantity;
        IncrementVersion();
    }

    private void ExpireSingleReservation(Reservation reservation)
    {
        reservation.Expire();
        AvailableQuantity += reservation.Quantity;
        ReservedQuantity -= reservation.Quantity;
        IncrementVersion();
    }

    private Reservation? FindReservation(ReservationId reservationId) =>
        _reservations.FirstOrDefault(r => r.Id == reservationId);
}
