using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.ValueObjects;

public class Plan : ValueObject
{
    public string Name { get; }
    public Money MonthlyPrice { get; }
    public int Tier { get; }

    private Plan(string name, Money monthlyPrice, int tier)
    {
        Name = name;
        MonthlyPrice = monthlyPrice;
        Tier = tier;
    }

    public static Plan Basic => new("Basic", Money.Create(9m, "USD").Value, 1);
    public static Plan Pro => new("Pro", Money.Create(29m, "USD").Value, 2);
    public static Plan Enterprise => new("Enterprise", Money.Create(99m, "USD").Value, 3);

    public bool IsUpgradeFrom(Plan other) => Tier > other.Tier;
    public bool IsDowngradeFrom(Plan other) => Tier < other.Tier;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Tier;
    }
}
