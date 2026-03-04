using InventoryReservation.Domain.Common;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Entities;

public class Reservation : Entity<Guid>
{
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Reservation() // EF Core
    {
        ProductId = null!;
        Quantity = null!;
    }

    private Reservation(Guid id, ProductId productId, Quantity quantity, DateTime expiresAt)
        : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Reservation> Create(ProductId productId, Quantity quantity, DateTime expiresAt)
    {
        if (quantity.IsZero)
            return Result.Failure<Reservation>("Reservation quantity must be positive");

        if (expiresAt <= DateTime.UtcNow)
            return Result.Failure<Reservation>("Expiry must be in the future");

        return Result.Success(new Reservation(Guid.NewGuid(), productId, quantity, expiresAt));
    }

    public Result Confirm()
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot confirm reservation in {Status} status. Only Pending reservations can be confirmed");

        Status = ReservationStatus.Confirmed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot cancel reservation in {Status} status. Only Pending reservations can be cancelled");

        Status = ReservationStatus.Cancelled;
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot expire reservation in {Status} status. Only Pending reservations can be expired");

        Status = ReservationStatus.Expired;
        return Result.Success();
    }

    public bool IsExpired(DateTime asOf)
    {
        return Status == ReservationStatus.Pending && asOf >= ExpiresAt;
    }
}
