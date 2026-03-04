using SubscriptionManagement.Domain.ValueObjects;

namespace SubscriptionManagement.Tests.Unit;

public class PlanTests
{
    [Theory]
    [InlineData("Basic", 9)]
    [InlineData("Pro", 29)]
    [InlineData("Enterprise", 99)]
    public void PredefinedPlans_HaveCorrectPricing(string planName, decimal expectedPrice)
    {
        var plan = planName switch
        {
            "Basic" => Plan.Basic,
            "Pro" => Plan.Pro,
            "Enterprise" => Plan.Enterprise,
            _ => throw new ArgumentException($"Unknown plan: {planName}")
        };

        Assert.Equal(planName, plan.Name);
        Assert.Equal(expectedPrice, plan.MonthlyPrice.Amount);
        Assert.Equal("USD", plan.MonthlyPrice.Currency);
    }

    [Fact]
    public void IsUpgradeFrom_LowerToHigherPlan_ReturnsTrue()
    {
        Assert.True(Plan.Pro.IsUpgradeFrom(Plan.Basic));
        Assert.True(Plan.Enterprise.IsUpgradeFrom(Plan.Basic));
        Assert.True(Plan.Enterprise.IsUpgradeFrom(Plan.Pro));
    }

    [Fact]
    public void IsUpgradeFrom_HigherToLowerPlan_ReturnsFalse()
    {
        Assert.False(Plan.Basic.IsUpgradeFrom(Plan.Pro));
        Assert.False(Plan.Basic.IsUpgradeFrom(Plan.Enterprise));
        Assert.False(Plan.Pro.IsUpgradeFrom(Plan.Enterprise));
    }

    [Fact]
    public void IsUpgradeFrom_SamePlan_ReturnsFalse()
    {
        Assert.False(Plan.Basic.IsUpgradeFrom(Plan.Basic));
        Assert.False(Plan.Pro.IsUpgradeFrom(Plan.Pro));
    }

    [Fact]
    public void IsDowngradeFrom_HigherToLowerPlan_ReturnsTrue()
    {
        Assert.True(Plan.Basic.IsDowngradeFrom(Plan.Pro));
        Assert.True(Plan.Basic.IsDowngradeFrom(Plan.Enterprise));
        Assert.True(Plan.Pro.IsDowngradeFrom(Plan.Enterprise));
    }

    [Fact]
    public void Equals_SamePlan_ReturnsTrue()
    {
        var plan1 = Plan.Basic;
        var plan2 = Plan.Basic;

        Assert.Equal(plan1, plan2);
    }
}
