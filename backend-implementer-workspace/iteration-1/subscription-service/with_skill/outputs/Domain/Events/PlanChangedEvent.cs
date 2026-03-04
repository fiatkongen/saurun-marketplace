using SubscriptionManagement.Domain.Common;
using SubscriptionManagement.Domain.Enums;

namespace SubscriptionManagement.Domain.Events;

public record PlanChangedEvent(
    Guid SubscriptionId,
    PlanType OldPlan,
    PlanType NewPlan,
    bool Immediate,
    DateTime OccurredAt
) : IDomainEvent;
