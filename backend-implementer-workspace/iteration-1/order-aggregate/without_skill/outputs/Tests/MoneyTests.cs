using Domain;
using Xunit;

namespace Tests;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ReturnsSuccess()
    {
        var result = Money.Create(100m, Currency.USD);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value!.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void Create_WithZeroAmount_ReturnsSuccess()
    {
        var result = Money.Create(0m, Currency.USD);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.Amount);
    }

    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        var result = Money.Create(-1m, Currency.USD);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error!);
    }

    [Fact]
    public void Create_RoundsToTwoDecimalPlaces()
    {
        var result = Money.Create(10.999m, Currency.USD);

        Assert.True(result.IsSuccess);
        Assert.Equal(11.00m, result.Value!.Amount);
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSuccess()
    {
        var a = Money.Create(10m, Currency.USD).Value!;
        var b = Money.Create(20m, Currency.USD).Value!;

        var result = a.Add(b);

        Assert.True(result.IsSuccess);
        Assert.Equal(30m, result.Value!.Amount);
        Assert.Equal(Currency.USD, result.Value.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_ReturnsFailure()
    {
        var usd = Money.Create(10m, Currency.USD).Value!;
        var eur = Money.Create(20m, Currency.EUR).Value!;

        var result = usd.Add(eur);

        Assert.True(result.IsFailure);
        Assert.Contains("Cannot add", result.Error!);
    }

    [Fact]
    public void Multiply_ByPositiveFactor_ReturnsCorrectResult()
    {
        var money = Money.Create(25.50m, Currency.USD).Value!;

        var result = money.Multiply(3);

        Assert.True(result.IsSuccess);
        Assert.Equal(76.50m, result.Value!.Amount);
    }

    [Fact]
    public void Multiply_ByZero_ReturnsZero()
    {
        var money = Money.Create(25.50m, Currency.USD).Value!;

        var result = money.Multiply(0);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.Amount);
    }

    [Fact]
    public void Multiply_ByNegativeFactor_ReturnsFailure()
    {
        var money = Money.Create(25.50m, Currency.USD).Value!;

        var result = money.Multiply(-1);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error!);
    }

    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmount()
    {
        var zero = Money.Zero(Currency.EUR);

        Assert.Equal(0m, zero.Amount);
        Assert.Equal(Currency.EUR, zero.Currency);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.Create(10m, Currency.USD).Value!;
        var b = Money.Create(10m, Currency.USD).Value!;

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        var a = Money.Create(10m, Currency.USD).Value!;
        var b = Money.Create(20m, Currency.USD).Value!;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equality_DifferentCurrency_AreNotEqual()
    {
        var a = Money.Create(10m, Currency.USD).Value!;
        var b = Money.Create(10m, Currency.EUR).Value!;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var money = Money.Create(99.95m, Currency.USD).Value!;

        Assert.Equal("99.95 USD", money.ToString());
    }
}
