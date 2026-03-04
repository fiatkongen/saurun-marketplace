namespace Domain;

/// <summary>
/// Value object representing a monetary amount with currency.
/// Immutable. Two Money instances are equal if both amount and currency match.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a Money value object. Amount must be non-negative.
    /// </summary>
    public static Result<Money> Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            return Result.Failure<Money>("Money amount cannot be negative.");

        return Result.Success(new Money(Math.Round(amount, 2), currency));
    }

    /// <summary>
    /// Returns a zero-value Money in the specified currency.
    /// </summary>
    public static Money Zero(Currency currency) => new(0m, currency);

    /// <summary>
    /// Adds two Money values. They must share the same currency.
    /// </summary>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result.Failure<Money>($"Cannot add {Currency} and {other.Currency}.");

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies money by a scalar quantity.
    /// </summary>
    public Result<Money> Multiply(int factor)
    {
        if (factor < 0)
            return Result.Failure<Money>("Cannot multiply money by a negative factor.");

        return Create(Amount * factor, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
