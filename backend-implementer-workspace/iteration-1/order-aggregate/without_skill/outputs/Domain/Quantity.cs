namespace Domain;

public sealed record Quantity
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Result<Quantity> Create(int value)
    {
        if (value < 1)
            return Result<Quantity>.Failure("Quantity must be at least 1.");

        return Result<Quantity>.Success(new Quantity(value));
    }

    public override string ToString() => Value.ToString();
}
