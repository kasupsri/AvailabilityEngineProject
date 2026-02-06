namespace AvailabilityEngineProject.Infrastructure.Persistence.Entity;

public class Person
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
}
