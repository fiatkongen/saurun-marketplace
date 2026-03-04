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
        ReservedQuantity = Quantity.Zero;
        Version = 0;
    }

    public static Result<InventoryItem> Create(ProductId productId, Quantity availableQuantity)
    {
        var item = new InventoryItem(Guid.NewGuid(), productId, availableQuantity);
        item.AddDomainEvent(new InventoryItemCreatedEvent(
            item.Id,
            productId.Value,
            availableQuantity.Value,
            DateTime.UtcNow));
        return Result.Success(item);
    }

    public Result<ReservationResult> Reserve(Quantity requestedQuantity, DateTime expiresAt)
    {
        if (requestedQuantity.IsZero)
            return Result.Failure<ReservationResult>("Reservation quantity must be positive");

        // If nothing available, return empty result without creating a reservation
        if (AvailableQuantity.IsZero)
        {
            return Result.Success(new ReservationResult(
                Quantity.Zero,
                requestedQuantity,
                null));
        }

        // Determine how much we can actually reserve
        var actualReserved = AvailableQuantity.Value >= requestedQuantity.Value
            ? requestedQuantity
            : AvailableQuantity;

        var shortfall = Quantity.Create(requestedQuantity.Value - actualReserved.Value).Value;

        // Create the reservation entity
        var reservationResult = Reservation.Create(ProductId, actualReserved, expiresAt);
        if (reservationResult.IsFailure)
            return Result.Failure<ReservationResult>(reservationResult.Error);

        var reservation = reservationResult.Value;
        _reservations.Add(reservation);

        // Update quantities
        AvailableQuantity = AvailableQuantity.Subtract(actualReserved).Value;
        ReservedQuantity = ReservedQuantity.Add(actualReserved);
        Version++;

        AddDomainEvent(new StockReservedEvent(
            Id,
            reservation.Id,
            actualReserved.Value,
            shortfall.Value,
            DateTime.UtcNow));

        return Result.Success(new ReservationResult(actualReserved, shortfall, reservation));
    }

    public Result ConfirmReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return Result.Failure("Reservation not found");

        var confirmResult = reservation.Confirm();
        if (confirmResult.IsFailure)
            return confirmResult;

        // Confirming permanently removes from reserved (available already decreased at reserve time)
        ReservedQuantity = ReservedQuantity.Subtract(reservation.Quantity).Value;
        Version++;

        AddDomainEvent(new ReservationConfirmedEvent(
            Id,
            reservationId,
            reservation.Quantity.Value,
            DateTime.UtcNow));

        return Result.Success();
    }

    public Result CancelReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return Result.Failure("Reservation not found");

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;

        // Cancelling returns stock to available
        AvailableQuantity = AvailableQuantity.Add(reservation.Quantity);
        ReservedQuantity = ReservedQuantity.Subtract(reservation.Quantity).Value;
        Version++;

        AddDomainEvent(new ReservationCancelledEvent(
            Id,
            reservationId,
            reservation.Quantity.Value,
            DateTime.UtcNow));

        return Result.Success();
    }

    public Result ExpireReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return Result.Failure("Reservation not found");

        var expireResult = reservation.Expire();
        if (expireResult.IsFailure)
            return expireResult;

        // Expiring returns stock to available (same as cancel)
        AvailableQuantity = AvailableQuantity.Add(reservation.Quantity);
        ReservedQuantity = ReservedQuantity.Subtract(reservation.Quantity).Value;
        Version++;

        AddDomainEvent(new ReservationExpiredEvent(
            Id,
            reservationId,
            reservation.Quantity.Value,
            DateTime.UtcNow));

        return Result.Success();
    }

    public int ExpireOverdueReservations(DateTime asOf)
    {
        var overdueReservations = _reservations
            .Where(r => r.IsExpired(asOf))
            .ToList();

        foreach (var reservation in overdueReservations)
        {
            ExpireReservation(reservation.Id);
        }

        return overdueReservations.Count;
    }
}
