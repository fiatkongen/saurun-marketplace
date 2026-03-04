namespace InventoryReservation.Domain.ValueObjects;

public sealed record ReservationId
{
    public Guid Value { get; }

    public ReservationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReservationId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ReservationId Create() => new(Guid.NewGuid());
    public static ReservationId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
