using SubscriptionManagement.Domain.Enums;
using SubscriptionManagement.Domain.ValueObjects;
using Xunit;

namespace SubscriptionManagement.Tests;

public class PlanTests
{
    [Theory]
    [InlineData(PlanType.Basic, 9)]
    [InlineData(PlanType.Pro, 29)]
    [InlineData(PlanType.Enterprise, 99)]
    public void FromType_ReturnsCorrectPlanWithPrice(PlanType type, decimal expectedPrice)
    {
        var result = Plan.FromType(type);

        Assert.True(result.IsSuccess);
        Assert.Equal(type, result.Value.Type);
        Assert.Equal(expectedPrice, result.Value.MonthlyPrice.Amount);
    }

    [Fact]
    public void BasicPlan_HasCorrectPrice()
    {
        Assert.Equal(9m, Plan.Basic.MonthlyPrice.Amount);
        Assert.Equal("USD", Plan.Basic.MonthlyPrice.Currency);
    }

    [Fact]
    public void ProPlan_HasCorrectPrice()
    {
        Assert.Equal(29m, Plan.Pro.MonthlyPrice.Amount);
        Assert.Equal("USD", Plan.Pro.MonthlyPrice.Currency);
    }

    [Fact]
    public void EnterprisePlan_HasCorrectPrice()
    {
        Assert.Equal(99m, Plan.Enterprise.MonthlyPrice.Amount);
        Assert.Equal("USD", Plan.Enterprise.MonthlyPrice.Currency);
    }

    [Fact]
    public void IsUpgradeFrom_ProFromBasic_ReturnsTrue()
    {
        Assert.True(Plan.Pro.IsUpgradeFrom(Plan.Basic));
    }

    [Fact]
    public void IsUpgradeFrom_BasicFromPro_ReturnsFalse()
    {
        Assert.False(Plan.Basic.IsUpgradeFrom(Plan.Pro));
    }

    [Fact]
    public void IsDowngradeFrom_BasicFromPro_ReturnsTrue()
    {
        Assert.True(Plan.Basic.IsDowngradeFrom(Plan.Pro));
    }

    [Fact]
    public void IsDowngradeFrom_ProFromBasic_ReturnsFalse()
    {
        Assert.False(Plan.Pro.IsDowngradeFrom(Plan.Basic));
    }

    [Fact]
    public void IsUpgradeFrom_EnterpriseFromBasic_ReturnsTrue()
    {
        Assert.True(Plan.Enterprise.IsUpgradeFrom(Plan.Basic));
    }

    [Fact]
    public void Equals_SameType_ReturnsTrue()
    {
        var plan1 = Plan.FromType(PlanType.Pro).Value;
        var plan2 = Plan.FromType(PlanType.Pro).Value;

        Assert.Equal(plan1, plan2);
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        Assert.NotEqual(Plan.Basic, Plan.Pro);
    }
}
