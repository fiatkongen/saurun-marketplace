using SubscriptionManagement.Domain;
using Xunit;

namespace SubscriptionManagement.Tests;

public class PlanTests
{
    [Theory]
    [InlineData(PlanType.Basic, "Basic", 9)]
    [InlineData(PlanType.Pro, "Pro", 29)]
    [InlineData(PlanType.Enterprise, "Enterprise", 99)]
    public void FromType_Returns_Correct_Plan(PlanType type, string expectedName, decimal expectedPrice)
    {
        var plan = Plan.FromType(type);

        Assert.Equal(expectedName, plan.Name);
        Assert.Equal(expectedPrice, plan.MonthlyPriceUsd);
    }

    [Fact]
    public void IsUpgradeFrom_Basic_To_Pro()
    {
        Assert.True(Plan.Pro.IsUpgradeFrom(Plan.Basic));
        Assert.False(Plan.Basic.IsUpgradeFrom(Plan.Pro));
    }

    [Fact]
    public void IsDowngradeFrom_Enterprise_To_Basic()
    {
        Assert.True(Plan.Basic.IsDowngradeFrom(Plan.Enterprise));
        Assert.False(Plan.Enterprise.IsDowngradeFrom(Plan.Basic));
    }

    [Fact]
    public void IsUpgrade_And_IsDowngrade_Are_Inverse()
    {
        Assert.True(Plan.Enterprise.IsUpgradeFrom(Plan.Basic));
        Assert.True(Plan.Basic.IsDowngradeFrom(Plan.Enterprise));
    }
}
