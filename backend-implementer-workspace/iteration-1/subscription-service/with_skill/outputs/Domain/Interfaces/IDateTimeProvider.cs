namespace SubscriptionManagement.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
