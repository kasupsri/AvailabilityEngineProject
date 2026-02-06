namespace AvailabilityEngineProject.Domain;

public sealed record TimeInterval(DateTimeOffset Start, DateTimeOffset End)
{
    public bool IsValid => End > Start;
}
