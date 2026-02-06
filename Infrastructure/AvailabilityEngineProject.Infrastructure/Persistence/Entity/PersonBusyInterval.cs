namespace AvailabilityEngineProject.Infrastructure.Persistence.Entity;

public class PersonBusyInterval
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
}
