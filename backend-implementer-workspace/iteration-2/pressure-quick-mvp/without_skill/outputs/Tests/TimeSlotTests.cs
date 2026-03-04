using Domain;
using Xunit;

namespace Tests;

public class TimeSlotTests
{
    private static TimeSlot CreateSlot(int capacity = 2) =>
        new(DateOnly.FromDateTime(DateTime.Today), new TimeOnly(9, 0), new TimeOnly(10, 0), capacity);

    [Fact]
    public void New_slot_has_full_capacity()
    {
        var slot = CreateSlot(3);
        Assert.Equal(3, slot.RemainingCapacity);
        Assert.Empty(slot.Bookings);
    }

    [Fact]
    public void Booking_reduces_remaining_capacity()
    {
        var slot = CreateSlot(2);
        slot.Book("Alice");
        Assert.Equal(1, slot.RemainingCapacity);
        Assert.Single(slot.Bookings);
    }

    [Fact]
    public void Book_returns_booking_with_correct_data()
    {
        var slot = CreateSlot();
        var booking = slot.Book("Bob");

        Assert.Equal("Bob", booking.CustomerName);
        Assert.Equal(slot.Id, booking.TimeSlotId);
        Assert.NotEqual(Guid.Empty, booking.Id);
    }

    [Fact]
    public void Cannot_book_when_full()
    {
        var slot = CreateSlot(1);
        slot.Book("Alice");

        var ex = Assert.Throws<InvalidOperationException>(() => slot.Book("Bob"));
        Assert.Contains("No capacity", ex.Message);
    }

    [Fact]
    public void Multiple_bookings_up_to_capacity_succeed()
    {
        var slot = CreateSlot(3);
        slot.Book("A");
        slot.Book("B");
        slot.Book("C");

        Assert.Equal(0, slot.RemainingCapacity);
        Assert.Equal(3, slot.Bookings.Count);
    }

    [Fact]
    public void Cannot_book_with_empty_name()
    {
        var slot = CreateSlot();
        Assert.Throws<ArgumentException>(() => slot.Book(""));
        Assert.Throws<ArgumentException>(() => slot.Book("   "));
    }

    [Fact]
    public void EndTime_must_be_after_StartTime()
    {
        Assert.Throws<ArgumentException>(() =>
            new TimeSlot(DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0), new TimeOnly(9, 0), 5));
    }

    [Fact]
    public void Equal_start_and_end_time_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new TimeSlot(DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0), new TimeOnly(10, 0), 5));
    }

    [Fact]
    public void Zero_capacity_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new TimeSlot(DateOnly.FromDateTime(DateTime.Today), new TimeOnly(9, 0), new TimeOnly(10, 0), 0));
    }

    [Fact]
    public void Negative_capacity_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new TimeSlot(DateOnly.FromDateTime(DateTime.Today), new TimeOnly(9, 0), new TimeOnly(10, 0), -1));
    }
}
