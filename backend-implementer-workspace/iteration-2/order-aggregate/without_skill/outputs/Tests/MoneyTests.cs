using Domain;
using Xunit;

namespace Tests;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_Succeeds()
    {
        var result = Money.Create(10.50m, Currency.USD);

        Assert.True(result.IsSuccess);
        Assert.Equal(10.50m, result.Value.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void Create_WithZeroAmount_Succeeds()
    {
        var result = Money.Create(0m, Currency.EUR);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Amount);
    }

    [Fact]
    public void Create_WithNegativeAmount_Fails()
    {
        var result = Money.Create(-1m, Currency.USD);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        var result = Money.Create(10.555m, Currency.USD);

        Assert.True(result.IsSuccess);
        Assert.Equal(10.56m, result.Value.Amount);
    }

    [Fact]
    public void Zero_ReturnsZeroAmountInCurrency()
    {
        var money = Money.Zero(Currency.DKK);

        Assert.Equal(0m, money.Amount);
        Assert.Equal(Currency.DKK, money.Currency);
    }

    [Fact]
    public void Add_SameCurrency_Succeeds()
    {
        var a = Money.Create(5m, Currency.USD).Value;
        var b = Money.Create(3.50m, Currency.USD).Value;

        var result = a.Add(b);

        Assert.True(result.IsSuccess);
        Assert.Equal(8.50m, result.Value.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_Fails()
    {
        var usd = Money.Create(5m, Currency.USD).Value;
        var eur = Money.Create(3m, Currency.EUR).Value;

        var result = usd.Add(eur);

        Assert.True(result.IsFailure);
        Assert.Contains("USD", result.Error!);
        Assert.Contains("EUR", result.Error!);
    }

    [Fact]
    public void Multiply_ByPositiveFactor_Succeeds()
    {
        var money = Money.Create(25.00m, Currency.USD).Value;

        var result = money.Multiply(3);

        Assert.True(result.IsSuccess);
        Assert.Equal(75.00m, result.Value.Amount);
    }

    [Fact]
    public void Multiply_ByZero_ReturnsZero()
    {
        var money = Money.Create(25.00m, Currency.USD).Value;

        var result = money.Multiply(0);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Amount);
    }

    [Fact]
    public void Multiply_ByNegativeFactor_Fails()
    {
        var money = Money.Create(25.00m, Currency.USD).Value;

        var result = money.Multiply(-1);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.Create(10m, Currency.USD).Value;
        var b = Money.Create(10m, Currency.USD).Value;

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        var a = Money.Create(10m, Currency.USD).Value;
        var b = Money.Create(20m, Currency.USD).Value;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equality_DifferentCurrency_AreNotEqual()
    {
        var a = Money.Create(10m, Currency.USD).Value;
        var b = Money.Create(10m, Currency.EUR).Value;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var money = Money.Create(99.99m, Currency.GBP).Value;

        Assert.Equal("99.99 GBP", money.ToString());
    }
}
