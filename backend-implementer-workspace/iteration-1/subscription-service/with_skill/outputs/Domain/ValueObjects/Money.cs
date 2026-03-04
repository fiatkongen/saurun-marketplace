using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency is required");
        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
