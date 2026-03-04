namespace Domain;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid TimeSlotId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Booking() { } // EF

    public Booking(Guid timeSlotId, string customerName)
    {
        Id = Guid.NewGuid();
        TimeSlotId = timeSlotId;
        CustomerName = customerName;
        CreatedAt = DateTime.UtcNow;
    }
}
