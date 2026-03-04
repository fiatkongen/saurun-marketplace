using Domain.Common;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities;

public class BookableSlot : AggregateRoot<Guid>
{
    public TimeSlot Slot { get; private set; }
    public Capacity Capacity { get; private set; }

    private readonly List<Booking> _bookings = new();
    public IReadOnlyList<Booking> Bookings => _bookings.AsReadOnly();

    public int RemainingCapacity => Capacity.MaxCapacity - _bookings.Count;

    private BookableSlot() { } // EF Core

    private BookableSlot(Guid id, TimeSlot slot, Capacity capacity) : base(id)
    {
        Slot = slot;
        Capacity = capacity;
    }

    public static Result<BookableSlot> Create(TimeSlot slot, Capacity capacity)
    {
        var bookableSlot = new BookableSlot(Guid.NewGuid(), slot, capacity);
        bookableSlot.AddDomainEvent(new SlotCreatedEvent(bookableSlot.Id, DateTime.UtcNow));
        return Result.Success(bookableSlot);
    }

    public Result Book(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return Result.Failure("Customer ID is required");

        if (_bookings.Any(b => b.CustomerId == customerId))
            return Result.Failure("Customer has already booked this slot");

        if (_bookings.Count >= Capacity.MaxCapacity)
            return Result.Failure("No capacity available for this slot");

        var booking = new Booking(Guid.NewGuid(), customerId, DateTime.UtcNow);
        _bookings.Add(booking);
        AddDomainEvent(new BookingCreatedEvent(booking.Id, Id, customerId, DateTime.UtcNow));

        return Result.Success();
    }
}
