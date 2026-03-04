using InventoryReservation.Domain.Common;

namespace InventoryReservation.Domain.ValueObjects;

public class ProductId : ValueObject
{
    public Guid Value { get; }

    private ProductId(Guid value)
    {
        Value = value;
    }

    public static Result<ProductId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<ProductId>("ProductId cannot be empty");
        return Result.Success(new ProductId(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
