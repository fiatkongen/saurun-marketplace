using SubscriptionManagement.Domain.Common;

namespace SubscriptionManagement.Domain.Events;

public record PlanChangedEvent(
    Guid SubscriptionId,
    string FromPlan,
    string ToPlan,
    bool IsImmediate,
    DateTime OccurredAt
) : IDomainEvent;
