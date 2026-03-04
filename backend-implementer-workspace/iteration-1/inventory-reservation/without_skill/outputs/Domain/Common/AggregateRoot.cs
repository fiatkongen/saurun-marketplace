namespace InventoryReservation.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    public int Version { get; protected set; }

    protected void IncrementVersion() => Version++;
}
