namespace InventoryReservation.Domain.ValueObjects;

public sealed record Quantity
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

    public static Quantity operator +(Quantity left, Quantity right) =>
        new(left.Value + right.Value);

    public static Quantity operator -(Quantity left, Quantity right)
    {
        var result = left.Value - right.Value;
        if (result < 0)
            throw new InvalidOperationException(
                $"Cannot subtract {right.Value} from {left.Value}: result would be negative.");
        return new(result);
    }

    public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;
    public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;
    public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;
    public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;

    public Quantity Min(Quantity other) => Value <= other.Value ? this : other;

    public override string ToString() => Value.ToString();
}
