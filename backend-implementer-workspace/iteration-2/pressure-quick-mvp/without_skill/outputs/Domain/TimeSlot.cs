namespace Domain;

public class TimeSlot
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int MaxCapacity { get; private set; }

    private readonly List<Booking> _bookings = new();
    public IReadOnlyList<Booking> Bookings => _bookings.AsReadOnly();

    public int RemainingCapacity => MaxCapacity - _bookings.Count;

    private TimeSlot() { } // EF

    public TimeSlot(DateOnly date, TimeOnly startTime, TimeOnly endTime, int maxCapacity)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.");
        if (maxCapacity <= 0)
            throw new ArgumentException("Max capacity must be positive.");

        Id = Guid.NewGuid();
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        MaxCapacity = maxCapacity;
    }

    public Booking Book(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.");
        if (RemainingCapacity <= 0)
            throw new InvalidOperationException("No capacity remaining for this time slot.");

        var booking = new Booking(Id, customerName);
        _bookings.Add(booking);
        return booking;
    }
}
