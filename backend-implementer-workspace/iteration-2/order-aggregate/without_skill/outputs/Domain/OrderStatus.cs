namespace Domain;

/// <summary>
/// Represents the lifecycle status of an Order.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order is being composed; items can be added/removed.</summary>
    Draft,

    /// <summary>Order has been confirmed; no further modifications allowed.</summary>
    Confirmed
}
