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
    /// Reserves up to the requested quantity. Supports partial reservations.
    /// Returns a ReservationResult indicating how much was actually reserved and any shortfall.
    /// </summary>
    public Result<ReservationResult> Reserve(Quantity requestedQuantity, DateTime utcNow, TimeSpan? expirationWindow = null)
    {
        if (requestedQuantity <= Quantity.Zero)
            return Result.Failure<ReservationResult>("Requested quantity must be greater than zero.");

        // Expire any pending reservations that have passed their expiry
        ExpireOverdueReservations(utcNow);

        var effectiveAvailable = AvailableQuantity;
        if (effectiveAvailable <= Quantity.Zero)
            return Result.Failure<ReservationResult>("No stock available for reservation.");

        var actualReserve = effectiveAvailable >= requestedQuantity
            ? requestedQuantity
            : effectiveAvailable;

        var expiration = expirationWindow ?? TimeSpan.FromMinutes(15);
        var reservationId = ReservationId.Create();
        var reservation = new Reservation(reservationId, Id, actualReserve, utcNow.Add(expiration));

        _reservations.Add(reservation);
        AvailableQuantity -= actualReserve;
        ReservedQuantity += actualReserve;
        IncrementVersion();

        var result = new ReservationResult(reservationId, actualReserve, requestedQuantity);
        return Result.Success(result);
    }

    /// <summary>
    /// Confirms a pending reservation. Permanently decreases stock (reserved becomes consumed).
    /// </summary>
    public Result ConfirmReservation(ReservationId reservationId, DateTime utcNow)
    {
        var reservation = FindReservation(reservationId);
        if (reservation is null)
            return Result.Failure($"Reservation '{reservationId}' not found.");

        // Check expiry before confirming
        if (reservation.IsExpired(utcNow))
        {
            ReturnReservedStock(reservation);
            reservation.Expire(utcNow);
            IncrementVersion();
            return Result.Failure("Reservation has expired and stock has been returned.");
        }

        var confirmResult = reservation.Confirm(utcNow);
        if (confirmResult.IsFailure)
            return confirmResult;

        // Stock leaves the system permanently
        ReservedQuantity -= reservation.Quantity;
        IncrementVersion();

        return Result.Success();
    }

    /// <summary>
    /// Cancels a pending reservation, returning reserved stock to available.
    /// </summary>
    public Result CancelReservation(ReservationId reservationId)
    {
        var reservation = FindReservation(reservationId);
        if (reservation is null)
            return Result.Failure($"Reservation '{reservationId}' not found.");

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;

        ReturnReservedStock(reservation);
        IncrementVersion();

        return Result.Success();
    }

    /// <summary>
    /// Processes all overdue pending reservations, returning their stock to available.
    /// Returns the number of reservations expired.
    /// </summary>
    public int ExpireOverdueReservations(DateTime utcNow)
    {
        var expired = 0;
        var overdueReservations = _reservations
            .Where(r => r.Status == ReservationStatus.Pending && r.IsExpired(utcNow))
            .ToList();

        foreach (var reservation in overdueReservations)
        {
            var result = reservation.Expire(utcNow);
            if (result.IsSuccess)
            {
                ReturnReservedStock(reservation);
                expired++;
            }
        }

        if (expired > 0)
            IncrementVersion();

        return expired;
    }

    /// <summary>
    /// Adds stock to this inventory item.
    /// </summary>
    public void Restock(Quantity quantity)
    {
        if (quantity <= Quantity.Zero)
            throw new ArgumentException("Restock quantity must be positive.");

        AvailableQuantity += quantity;
        IncrementVersion();
    }

    private Reservation? FindReservation(ReservationId reservationId)
        => _reservations.FirstOrDefault(r => r.Id == reservationId);

    private void ReturnReservedStock(Reservation reservation)
    {
        AvailableQuantity += reservation.Quantity;
        ReservedQuantity -= reservation.Quantity;
    }
}
