using Domain.Common;

namespace Domain.ValueObjects;

public class Capacity : ValueObject
{
    public int MaxCapacity { get; }

    private Capacity(int maxCapacity)
    {
        MaxCapacity = maxCapacity;
    }

    public static Result<Capacity> Create(int maxCapacity)
    {
        if (maxCapacity <= 0)
            return Result.Failure<Capacity>("Capacity must be a positive number");

        return Result.Success(new Capacity(maxCapacity));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxCapacity;
    }
}
