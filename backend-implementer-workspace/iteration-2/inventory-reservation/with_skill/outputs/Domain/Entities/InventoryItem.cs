using InventoryReservation.Domain.Common;
using InventoryReservation.Domain.Events;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Entities;

public class InventoryItem : AggregateRoot<Guid>
{
    public ProductId ProductId { get; private set; }
    public Quantity AvailableQuantity { get; private set; }
    public Quantity ReservedQuantity { get; private set; }
    public int Version { get; private set; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

    private InventoryItem() // EF Core
    {
        ProductId = null!;
        AvailableQuantity = null!;
        ReservedQuantity = null!;
    }

    private InventoryItem(Guid id, ProductId productId, Quantity availableQuantity)
        : base(id)
    {
        ProductId = productId;
        AvailableQuantity = availableQuantity;
        ReservedQuantity = Quantity.Zero();
        Version = 0;
    }

    public static Result<InventoryItem> Create(ProductId productId, Quantity initialQuantity)
    {
        var item = new InventoryItem(Guid.NewGuid(), productId, initialQuantity);
        item.AddDomainEvent(new InventoryItemCreatedEvent(
            item.Id, productId.Value, initialQuantity.Value, DateTime.UtcNow));
        return Result.Success(item);
    }

    /// <summary>
    /// Reserves stock. Supports partial reservations: if requested > available,
    /// reserves what is available and reports shortfall.
    /// Proactively expires overdue reservations before checking availability.
    /// </summary>
    public Result<ReservationOutcome> Reserve(Quantity requested, DateTime utcNow)
    {
        // Proactively expire overdue reservations to free up stock
        ExpireOverdueReservations(utcNow);

        var effectiveAvailable = AvailableQuantity.Value;

        if (effectiveAvailable <= 0)
            return Result.Failure<ReservationOutcome>("No stock available for reservation");

        int toReserve;
        int shortfall;

        if (requested.Value <= effectiveAvailable)
        {
            toReserve = requested.Value;
            shortfall = 0;
        }
        else
        {
            // Partial reservation: reserve what's available
            toReserve = effectiveAvailable;
            shortfall = requested.Value - effectiveAvailable;
        }

        var reserveQty = Quantity.FromInt(toReserve);
        var reservationResult = Reservation.Create(ProductId, reserveQty, utcNow);
        if (reservationResult.IsFailure)
            return Result.Failure<ReservationOutcome>(reservationResult.Error);

        var reservation = reservationResult.Value;
        _reservations.Add(reservation);

        AvailableQuantity = Quantity.FromInt(AvailableQuantity.Value - toReserve);
        ReservedQuantity = Quantity.FromInt(ReservedQuantity.Value + toReserve);
        Version++;

        AddDomainEvent(new StockReservedEvent(
            Id, reservation.Id, ProductId.Value, toReserve, shortfall, utcNow));

        if (shortfall == 0)
            return Result.Success(ReservationOutcome.FullyReserved(reservation));
        else
            return Result.Success(ReservationOutcome.PartiallyReserved(reservation, shortfall));
    }

    /// <summary>
    /// Confirms a reservation, permanently removing the stock from reserved quantity.
    /// Available quantity stays unchanged (already decremented at reservation time).
    /// </summary>
    public Result ConfirmReservation(Guid reservationId, DateTime utcNow)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return Result.Failure("Reservation not found");

        var confirmResult = reservation.Confirm(utcNow);
        if (confirmResult.IsFailure)
            return confirmResult;

        ReservedQuantity = Quantity.FromInt(ReservedQuantity.Value - reservation.Quantity.Value);
        Version++;

        AddDomainEvent(new ReservationConfirmedEvent(
            Id, reservationId, ProductId.Value, reservation.Quantity.Value, utcNow));

        return Result.Success();
    }

    /// <summary>
    /// Cancels a reservation, returning stock to available.
    /// </summary>
    public Result CancelReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return Result.Failure("Reservation not found");

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;

        ReturnStockFromReservation(reservation);
        Version++;

        AddDomainEvent(new ReservationCancelledEvent(
            Id, reservationId, ProductId.Value, reservation.Quantity.Value, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Expires all overdue pending reservations, returning their stock to available.
    /// Returns the count of expired reservations.
    /// </summary>
    public int ExpireOverdueReservations(DateTime utcNow)
    {
        var overdue = _reservations
            .Where(r => r.IsExpired(utcNow))
            .ToList();

        foreach (var reservation in overdue)
        {
            reservation.Expire(utcNow);
            ReturnStockFromReservation(reservation);

            AddDomainEvent(new ReservationExpiredEvent(
                Id, reservation.Id, ProductId.Value, reservation.Quantity.Value, utcNow));
        }

        return overdue.Count;
    }

    private void ReturnStockFromReservation(Reservation reservation)
    {
        AvailableQuantity = Quantity.FromInt(AvailableQuantity.Value + reservation.Quantity.Value);
        ReservedQuantity = Quantity.FromInt(ReservedQuantity.Value - reservation.Quantity.Value);
    }
}
