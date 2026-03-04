using Domain.Base;

namespace Domain.ValueObjects;

public class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Result<Quantity> Create(int value)
    {
        if (value <= 0)
            return Result.Failure<Quantity>("Quantity must be positive (1 or more)");
        return Result.Success(new Quantity(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
