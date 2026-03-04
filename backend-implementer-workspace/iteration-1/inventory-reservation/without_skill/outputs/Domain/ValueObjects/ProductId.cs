namespace InventoryReservation.Domain.ValueObjects;

public sealed record ProductId
{
    public Guid Value { get; }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ProductId Create() => new(Guid.NewGuid());
    public static ProductId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
