using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.ValueObjects;

public class Quantity : ValueObject, IComparable<Quantity>
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Result<Quantity> Create(int value)
    {
        if (value <= 0)
            return Result.Failure<Quantity>("Quantity must be positive");
        return Result.Success(new Quantity(value));
    }

    /// <summary>
    /// Creates a Quantity representing zero. Used internally for stock tracking.
    /// </summary>
    internal static Quantity Zero() => new(0);

    /// <summary>
    /// Creates a Quantity without validation. Used internally when value is known-safe.
    /// </summary>
    internal static Quantity FromInt(int value) => new(value);

    public Quantity Add(Quantity other) => new(Value + other.Value);

    public Result<Quantity> Subtract(Quantity other)
    {
        if (other.Value > Value)
            return Result.Failure<Quantity>("Cannot subtract more than available");
        return Result.Success(new Quantity(Value - other.Value));
    }

    public int CompareTo(Quantity? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator >=(Quantity left, Quantity right) =>
        left.CompareTo(right) >= 0;

    public static bool operator <=(Quantity left, Quantity right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(Quantity left, Quantity right) =>
        left.CompareTo(right) > 0;

    public static bool operator <(Quantity left, Quantity right) =>
        left.CompareTo(right) < 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
