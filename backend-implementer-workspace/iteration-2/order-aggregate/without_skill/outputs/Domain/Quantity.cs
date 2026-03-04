namespace Domain;

/// <summary>
/// Value object representing a positive quantity (1 or more).
/// </summary>
public sealed record Quantity
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Quantity. Value must be at least 1.
    /// </summary>
    public static Result<Quantity> Create(int value)
    {
        if (value < 1)
            return Result.Failure<Quantity>("Quantity must be at least 1.");

        return Result.Success(new Quantity(value));
    }

    public override string ToString() => Value.ToString();
}
