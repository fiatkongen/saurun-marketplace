namespace OrderAggregate.Domain;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
