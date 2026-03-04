using Domain.ValueObjects;

namespace Tests;

public class TimeSlotTests
{
    [Fact]
    public void Create_WithValidTimes_ReturnsSuccess()
    {
        var date = new DateOnly(2026, 3, 10);
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(10, 0);

        var result = TimeSlot.Create(date, start, end);

        Assert.True(result.IsSuccess);
        Assert.Equal(date, result.Value.Date);
        Assert.Equal(start, result.Value.StartTime);
        Assert.Equal(end, result.Value.EndTime);
    }

    [Fact]
    public void Create_WithEndBeforeStart_ReturnsFailure()
    {
        var date = new DateOnly(2026, 3, 10);
        var start = new TimeOnly(10, 0);
        var end = new TimeOnly(9, 0);

        var result = TimeSlot.Create(date, start, end);

        Assert.True(result.IsFailure);
        Assert.Contains("end time", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WithEqualStartAndEnd_ReturnsFailure()
    {
        var date = new DateOnly(2026, 3, 10);
        var time = new TimeOnly(9, 0);

        var result = TimeSlot.Create(date, time, time);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var date = new DateOnly(2026, 3, 10);
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(10, 0);

        var slot1 = TimeSlot.Create(date, start, end).Value;
        var slot2 = TimeSlot.Create(date, start, end).Value;

        Assert.Equal(slot1, slot2);
    }
}
