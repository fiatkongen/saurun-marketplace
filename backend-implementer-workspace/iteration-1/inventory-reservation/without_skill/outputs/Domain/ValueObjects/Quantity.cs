namespace InventoryReservation.Domain.ValueObjects;

public sealed record Quantity : IComparable<Quantity>
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(value));
        Value = value;
    }

    public static Quantity Zero => new(0);
    public static Quantity Of(int value) => new(value);

    public static Quantity operator +(Quantity a, Quantity b) => new(a.Value + b.Value);
    public static Quantity operator -(Quantity a, Quantity b) => new(a.Value - b.Value);

    public static bool operator >(Quantity a, Quantity b) => a.Value > b.Value;
    public static bool operator <(Quantity a, Quantity b) => a.Value < b.Value;
    public static bool operator >=(Quantity a, Quantity b) => a.Value >= b.Value;
    public static bool operator <=(Quantity a, Quantity b) => a.Value <= b.Value;

    public int CompareTo(Quantity? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
