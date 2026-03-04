using SubscriptionManagement.Domain;

namespace SubscriptionManagement.Tests;

public class PlanTests
{
    [Fact]
    public void Basic_HasCorrectPrice()
    {
        Assert.Equal(9.00m, Plan.Basic.MonthlyPriceUsd);
    }

    [Fact]
    public void Pro_HasCorrectPrice()
    {
        Assert.Equal(29.00m, Plan.Pro.MonthlyPriceUsd);
    }

    [Fact]
    public void Enterprise_HasCorrectPrice()
    {
        Assert.Equal(99.00m, Plan.Enterprise.MonthlyPriceUsd);
    }

    [Fact]
    public void All_ReturnsThreePlansInPriceOrder()
    {
        var plans = Plan.All;
        Assert.Equal(3, plans.Count);
        Assert.Equal(Plan.Basic, plans[0]);
        Assert.Equal(Plan.Pro, plans[1]);
        Assert.Equal(Plan.Enterprise, plans[2]);
    }

    [Theory]
    [InlineData("Basic")]
    [InlineData("basic")]
    [InlineData("BASIC")]
    public void FromName_CaseInsensitive_FindsPlan(string name)
    {
        var plan = Plan.FromName(name);
        Assert.NotNull(plan);
        Assert.Equal(Plan.Basic, plan);
    }

    [Fact]
    public void FromName_UnknownPlan_ReturnsNull()
    {
        Assert.Null(Plan.FromName("Unknown"));
    }

    [Fact]
    public void Pro_IsUpgradeFrom_Basic()
    {
        Assert.True(Plan.Pro.IsUpgradeFrom(Plan.Basic));
    }

    [Fact]
    public void Basic_IsDowngradeFrom_Pro()
    {
        Assert.True(Plan.Basic.IsDowngradeFrom(Plan.Pro));
    }

    [Fact]
    public void Basic_IsNotUpgradeFrom_Pro()
    {
        Assert.False(Plan.Basic.IsUpgradeFrom(Plan.Pro));
    }

    [Fact]
    public void Pro_IsNotDowngradeFrom_Basic()
    {
        Assert.False(Plan.Pro.IsDowngradeFrom(Plan.Basic));
    }

    [Fact]
    public void SamePlan_IsNeitherUpgradeNorDowngrade()
    {
        Assert.False(Plan.Pro.IsUpgradeFrom(Plan.Pro));
        Assert.False(Plan.Pro.IsDowngradeFrom(Plan.Pro));
    }
}
