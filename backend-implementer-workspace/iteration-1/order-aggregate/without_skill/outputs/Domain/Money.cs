namespace Domain;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            return Result<Money>.Failure("Money amount cannot be negative.");

        return Result<Money>.Success(new Money(Math.Round(amount, 2), currency));
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result<Money>.Failure($"Cannot add {other.Currency} to {Currency}.");

        return Result<Money>.Success(new Money(Amount + other.Amount, Currency));
    }

    public Result<Money> Multiply(int factor)
    {
        if (factor < 0)
            return Result<Money>.Failure("Cannot multiply money by a negative factor.");

        return Result<Money>.Success(new Money(Amount * factor, Currency));
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
