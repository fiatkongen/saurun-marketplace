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
    }

    public bool IsExpired(DateTime utcNow) =>
        Status == ReservationStatus.Pending && utcNow >= ExpiresAt;

    public bool IsPending => Status == ReservationStatus.Pending;

    internal void Confirm()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot confirm reservation in {Status} status.");
        Status = ReservationStatus.Confirmed;
    }

    internal void Cancel()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot cancel reservation in {Status} status.");
        Status = ReservationStatus.Cancelled;
    }

    internal void Expire()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot expire reservation in {Status} status.");
        Status = ReservationStatus.Expired;
    }
}
