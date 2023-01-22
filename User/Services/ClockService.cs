namespace User.Services;

public class ClockService : IClockService
{
    public DateTime NowUtc()
    {
        return DateTime.UtcNow;
    }
}