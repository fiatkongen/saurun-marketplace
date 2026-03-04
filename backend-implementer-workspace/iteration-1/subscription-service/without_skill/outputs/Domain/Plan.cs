namespace SubscriptionManagement.Domain;

public enum PlanType
{
    Basic,
    Pro,
    Enterprise
}

public sealed record Plan(PlanType Type, string Name, decimal MonthlyPriceUsd)
{
    public static readonly Plan Basic = new(PlanType.Basic, "Basic", 9m);
    public static readonly Plan Pro = new(PlanType.Pro, "Pro", 29m);
    public static readonly Plan Enterprise = new(PlanType.Enterprise, "Enterprise", 99m);

    public static Plan FromType(PlanType type) => type switch
    {
        PlanType.Basic => Basic,
        PlanType.Pro => Pro,
        PlanType.Enterprise => Enterprise,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public bool IsUpgradeFrom(Plan other) => MonthlyPriceUsd > other.MonthlyPriceUsd;
    public bool IsDowngradeFrom(Plan other) => MonthlyPriceUsd < other.MonthlyPriceUsd;
}
