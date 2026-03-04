using SubscriptionManagement.Domain.ValueObjects;

namespace SubscriptionManagement.Tests.Unit;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ReturnsSuccess()
    {
        var result = Money.Create(9.99m, "USD");

        Assert.True(result.IsSuccess);
        Assert.Equal(9.99m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        var result = Money.Create(-1m, "USD");

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WithEmptyCurrency_ReturnsFailure()
    {
        var result = Money.Create(10m, "");

        Assert.True(result.IsFailure);
        Assert.Contains("currency", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_NormalizesCurrencyToUpperCase()
    {
        var result = Money.Create(10m, "usd");

        Assert.True(result.IsSuccess);
        Assert.Equal("USD", result.Value.Currency);
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        var money1 = Money.Create(10m, "USD").Value;
        var money2 = Money.Create(10m, "USD").Value;

        Assert.Equal(money1, money2);
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        var money1 = Money.Create(10m, "USD").Value;
        var money2 = Money.Create(20m, "USD").Value;

        Assert.NotEqual(money1, money2);
    }
}
