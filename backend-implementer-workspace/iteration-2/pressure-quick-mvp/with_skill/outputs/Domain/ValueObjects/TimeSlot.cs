using Domain.Common;

namespace Domain.ValueObjects;

public class TimeSlot : ValueObject
{
    public DateOnly Date { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    private TimeSlot(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
    }

    public static Result<TimeSlot> Create(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            return Result.Failure<TimeSlot>("End time must be after start time");

        return Result.Success(new TimeSlot(date, startTime, endTime));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Date;
        yield return StartTime;
        yield return EndTime;
    }
}
