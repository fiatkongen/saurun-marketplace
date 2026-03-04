using InventoryReservation.Domain.Common;
using InventoryReservation.Domain.Enums;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Entities;

public class Reservation : Entity<ReservationId>
{
    public ProductId ProductId { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Reservation() { } // EF/serialization

    internal Reservation(
        ReservationId id,
        ProductId productId,
        Quantity quantity,
        DateTime expiresAt)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsExpired(DateTime utcNow) => Status == ReservationStatus.Pending && utcNow >= ExpiresAt;

    public Result Confirm(DateTime utcNow)
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot confirm reservation in '{Status}' status.");

        if (IsExpired(utcNow))
            return Result.Failure("Cannot confirm an expired reservation.");

        Status = ReservationStatus.Confirmed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot cancel reservation in '{Status}' status.");

        Status = ReservationStatus.Cancelled;
        return Result.Success();
    }

    public Result Expire(DateTime utcNow)
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot expire reservation in '{Status}' status.");

        if (!IsExpired(utcNow))
            return Result.Failure("Reservation has not yet expired.");

        Status = ReservationStatus.Expired;
        return Result.Success();
    }
}
