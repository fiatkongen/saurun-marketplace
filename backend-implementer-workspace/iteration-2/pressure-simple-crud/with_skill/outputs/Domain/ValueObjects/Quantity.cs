using Domain.Common;

namespace Domain.ValueObjects;

public class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    // EF Core
    private Quantity() { Value = 0; }

    public static Result<Quantity> Create(int value)
    {
        if (value < 0)
            return Result.Failure<Quantity>("Quantity cannot be negative");
        return Result.Success(new Quantity(value));
    }

    public Quantity Add(Quantity other) => new(Value + other.Value);

    public Result<Quantity> Subtract(Quantity other)
    {
        var diff = Value - other.Value;
        if (diff < 0)
            return Result.Failure<Quantity>("Subtraction would result in negative quantity");
        return Result.Success(new Quantity(diff));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
