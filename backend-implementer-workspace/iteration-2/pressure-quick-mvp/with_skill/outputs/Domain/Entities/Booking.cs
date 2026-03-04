using Domain.Common;

namespace Domain.Entities;

public class Booking : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Booking() { } // EF Core

    internal Booking(Guid id, Guid customerId, DateTime createdAt) : base(id)
    {
        CustomerId = customerId;
        CreatedAt = createdAt;
    }
}
