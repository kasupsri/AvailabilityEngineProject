namespace AvailabilityEngineProject.Domain.Models;

public class UserModel
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int Age { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Pincode { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
