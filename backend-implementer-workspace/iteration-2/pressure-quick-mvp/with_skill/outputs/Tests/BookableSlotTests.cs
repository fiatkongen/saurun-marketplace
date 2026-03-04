using Domain.Common;
using Domain.Entities;
using Domain.ValueObjects;

namespace Tests;

public class BookableSlotTests
{
    private static TimeSlot ValidSlot() =>
        TimeSlot.Create(new DateOnly(2026, 3, 10), new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;

    private static Capacity CapacityOf(int n) => Capacity.Create(n).Value;

    // --- Factory tests ---

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessWithCorrectProperties()
    {
        var slot = ValidSlot();
        var capacity = CapacityOf(5);

        var result = BookableSlot.Create(slot, capacity);

        Assert.True(result.IsSuccess);
        Assert.Equal(slot, result.Value.Slot);
        Assert.Equal(capacity, result.Value.Capacity);
    }

    [Fact]
    public void Create_RaisesSlotCreatedEvent()
    {
        var result = BookableSlot.Create(ValidSlot(), CapacityOf(3));

        Assert.Single(result.Value.DomainEvents);
    }

    // --- Booking tests ---

    [Fact]
    public void Book_WithCapacityAvailable_ReturnsSuccess()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(2)).Value;

        var result = slot.Book(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Single(slot.Bookings);
    }

    [Fact]
    public void Book_RecordsCustomerId()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(2)).Value;
        var customerId = Guid.NewGuid();

        slot.Book(customerId);

        Assert.Equal(customerId, slot.Bookings[0].CustomerId);
    }

    [Fact]
    public void Book_WhenAtCapacity_ReturnsFailure()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(1)).Value;
        slot.Book(Guid.NewGuid()); // Fill capacity

        var result = slot.Book(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Contains("capacity", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Book_MultipleCustomers_UntilFull_Works()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(3)).Value;

        Assert.True(slot.Book(Guid.NewGuid()).IsSuccess);
        Assert.True(slot.Book(Guid.NewGuid()).IsSuccess);
        Assert.True(slot.Book(Guid.NewGuid()).IsSuccess);
        Assert.True(slot.Book(Guid.NewGuid()).IsFailure);
    }

    [Fact]
    public void Book_WithEmptyCustomerId_ReturnsFailure()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(5)).Value;

        var result = slot.Book(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Contains("customer", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Book_SameCustomerTwice_ReturnsFailure()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(5)).Value;
        var customerId = Guid.NewGuid();
        slot.Book(customerId);

        var result = slot.Book(customerId);

        Assert.True(result.IsFailure);
        Assert.Contains("already", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Book_RaisesBookingCreatedEvent()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(5)).Value;
        slot.ClearDomainEvents(); // Clear creation event

        slot.Book(Guid.NewGuid());

        Assert.Single(slot.DomainEvents);
    }

    // --- RemainingCapacity ---

    [Fact]
    public void RemainingCapacity_ReflectsBookings()
    {
        var slot = BookableSlot.Create(ValidSlot(), CapacityOf(3)).Value;
        Assert.Equal(3, slot.RemainingCapacity);

        slot.Book(Guid.NewGuid());
        Assert.Equal(2, slot.RemainingCapacity);
    }
}
