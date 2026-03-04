using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.ValueObjects;

public class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    protected Quantity() { } // EF Core

    public static Result<Quantity> Create(int value)
    {
        if (value < 0)
            return Result.Failure<Quantity>("Quantity cannot be negative");

        return Result.Success(new Quantity(value));
    }

    public Quantity Add(Quantity other)
    {
        return new Quantity(Value + other.Value);
    }

    public Result<Quantity> Subtract(Quantity other)
    {
        if (other.Value > Value)
            return Result.Failure<Quantity>("Cannot subtract: insufficient quantity");

        return Result.Success(new Quantity(Value - other.Value));
    }

    public bool IsZero => Value == 0;

    public static Quantity Zero => new(0);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
