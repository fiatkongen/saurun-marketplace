namespace SubscriptionManagement.Domain;

public sealed record Plan
{
    public static readonly Plan Basic = new("Basic", 9.00m);
    public static readonly Plan Pro = new("Pro", 29.00m);
    public static readonly Plan Enterprise = new("Enterprise", 99.00m);

    public string Name { get; }
    public decimal MonthlyPriceUsd { get; }

    private Plan(string name, decimal monthlyPriceUsd)
    {
        Name = name;
        MonthlyPriceUsd = monthlyPriceUsd;
    }

    /// <summary>
    /// Returns all available plans ordered by price ascending.
    /// </summary>
    public static IReadOnlyList<Plan> All => [Basic, Pro, Enterprise];

    /// <summary>
    /// Finds a plan by name (case-insensitive). Returns null if not found.
    /// </summary>
    public static Plan? FromName(string name) =>
        All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public bool IsUpgradeFrom(Plan other) => MonthlyPriceUsd > other.MonthlyPriceUsd;
    public bool IsDowngradeFrom(Plan other) => MonthlyPriceUsd < other.MonthlyPriceUsd;
}
