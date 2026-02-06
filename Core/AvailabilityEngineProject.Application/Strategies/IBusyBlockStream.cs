using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Strategies;

public interface IBusyBlockStream
{
    Task<IReadOnlyList<TimeInterval>> GetBusyBlocksAsync(
        IReadOnlyList<string> attendees,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken);
}
