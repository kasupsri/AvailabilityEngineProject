namespace AvailabilityEngineProject.Application.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
}
