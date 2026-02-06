using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Queries.GetBusyInWindow;

public sealed record GetBusyInWindowRequest(
    IReadOnlyList<string> Attendees,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd);

public sealed record BusyInWindowAttendee(string Email, string Name, IReadOnlyList<TimeInterval> Busy);

public sealed record GetBusyInWindowResult(IReadOnlyList<BusyInWindowAttendee> Attendees);
