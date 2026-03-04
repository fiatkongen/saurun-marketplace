namespace Domain.Base;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
