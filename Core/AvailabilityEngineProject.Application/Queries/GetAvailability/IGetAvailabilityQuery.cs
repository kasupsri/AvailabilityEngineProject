using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Queries.GetAvailability;

public interface IGetAvailabilityQuery
{
    Task<GetAvailabilityResult> ExecuteAsync(GetAvailabilityRequest request, CancellationToken cancellationToken);
}

public sealed record GetAvailabilityRequest(
    IReadOnlyList<string> Attendees,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int DurationMinutes);

public sealed record GetAvailabilityResult(bool Found, TimeInterval? Slot);
