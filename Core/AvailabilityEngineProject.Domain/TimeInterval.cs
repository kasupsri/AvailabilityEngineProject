namespace AvailabilityEngineProject.Domain;

public sealed record TimeInterval(DateTimeOffset Start, DateTimeOffset End)
{
    public bool IsValid => End > Start;

    public static TimeInterval Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new ArgumentException("End must be after start.", nameof(end));
        return new TimeInterval(start, end);
    }
}
