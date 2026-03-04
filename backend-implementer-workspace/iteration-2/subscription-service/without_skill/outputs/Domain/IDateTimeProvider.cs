namespace SubscriptionManagement.Domain;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
