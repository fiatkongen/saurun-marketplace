using OrderAggregate.Domain;
using OrderAggregate.Domain.ValueObjects;

namespace OrderAggregate.Tests.Unit;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ReturnsSuccess()
    {
        var result = Money.Create(10.00m, "USD");

        Assert.True(result.IsSuccess);
        Assert.Equal(10.00m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }

    [Fact]
    public void Create_WithZeroAmount_ReturnsSuccess()
    {
        var result = Money.Create(0m, "USD");

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Amount);
    }

    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        var result = Money.Create(-1m, "USD");

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCurrency_ReturnsFailure(string? currency)
    {
        var result = Money.Create(10m, currency!);

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

    [Fact]
    public void Equals_DifferentCurrency_ReturnsFalse()
    {
        var money1 = Money.Create(10m, "USD").Value;
        var money2 = Money.Create(10m, "EUR").Value;

        Assert.NotEqual(money1, money2);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummedMoney()
    {
        var money1 = Money.Create(10m, "USD").Value;
        var money2 = Money.Create(20m, "USD").Value;

        var result = money1.Add(money2);

        Assert.True(result.IsSuccess);
        Assert.Equal(30m, result.Value.Amount);
        Assert.Equal("USD", result.Value.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_ReturnsFailure()
    {
        var money1 = Money.Create(10m, "USD").Value;
        var money2 = Money.Create(20m, "EUR").Value;

        var result = money1.Add(money2);

        Assert.True(result.IsFailure);
        Assert.Contains("currency", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}
