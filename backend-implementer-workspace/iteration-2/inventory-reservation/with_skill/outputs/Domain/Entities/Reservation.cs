using InventoryReservation.Domain.Common;
using InventoryReservation.Domain.ValueObjects;

namespace InventoryReservation.Domain.Entities;

public class Reservation : Entity<Guid>
{
    private const int ExpirationMinutes = 15;

    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public ReservationStatus Status { get; private set; }

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
    }

    public static Result<Reservation> Create(ProductId productId, Quantity quantity, DateTime utcNow)
    {
        var expiresAt = utcNow.AddMinutes(ExpirationMinutes);
        var reservation = new Reservation(Guid.NewGuid(), productId, quantity, expiresAt);
        return Result.Success(reservation);
    }

    public Result Confirm(DateTime utcNow)
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot confirm reservation in {Status} status");
        if (IsExpired(utcNow))
            return Result.Failure("Cannot confirm expired reservation");

        Status = ReservationStatus.Confirmed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot cancel reservation in {Status} status");

        Status = ReservationStatus.Cancelled;
        return Result.Success();
    }

    public Result Expire(DateTime utcNow)
    {
        if (Status != ReservationStatus.Pending)
            return Result.Failure($"Cannot expire reservation in {Status} status");
        if (!IsExpired(utcNow))
            return Result.Failure("Reservation has not yet expired");

        Status = ReservationStatus.Expired;
        return Result.Success();
    }

    public bool IsExpired(DateTime utcNow) =>
        Status == ReservationStatus.Pending && utcNow >= ExpiresAt;
}
