using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Enums;

namespace SubscriptionManagement.Domain.ValueObjects;

public class Plan : ValueObject
{
    public PlanType Type { get; }
    public Money MonthlyPrice { get; }

    private Plan(PlanType type, Money monthlyPrice)
    {
        Type = type;
        MonthlyPrice = monthlyPrice;
    }

    public static readonly Plan Basic = new(PlanType.Basic, Money.Create(9m, "USD").Value);
    public static readonly Plan Pro = new(PlanType.Pro, Money.Create(29m, "USD").Value);
    public static readonly Plan Enterprise = new(PlanType.Enterprise, Money.Create(99m, "USD").Value);

    public static Result<Plan> FromType(PlanType type)
    {
        return type switch
        {
            PlanType.Basic => Result.Success(Basic),
            PlanType.Pro => Result.Success(Pro),
            PlanType.Enterprise => Result.Success(Enterprise),
            _ => Result.Failure<Plan>($"Unknown plan type: {type}")
        };
    }

    public bool IsUpgradeFrom(Plan other) => Type > other.Type;
    public bool IsDowngradeFrom(Plan other) => Type < other.Type;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
    }
}
