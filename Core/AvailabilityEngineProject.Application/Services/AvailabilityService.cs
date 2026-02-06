using AvailabilityEngineProject.Application.Strategies;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Services;

public interface IAvailabilityService
{
    Task<TimeInterval?> FindEarliestSlotAsync(
        IReadOnlyList<string> attendees,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        TimeSpan duration,
        CancellationToken cancellationToken);
}

public sealed class AvailabilityService : IAvailabilityService
{
    private readonly IBusyBlockStream _busyBlockStream;

    public AvailabilityService(IBusyBlockStream busyBlockStream)
    {
        _busyBlockStream = busyBlockStream ?? throw new ArgumentNullException(nameof(busyBlockStream));
    }

    public async Task<TimeInterval?> FindEarliestSlotAsync(
        IReadOnlyList<string> attendees,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        if (windowEnd <= windowStart || duration <= TimeSpan.Zero)
            return null;
        if (windowStart + duration > windowEnd)
            return null;

        var blocks = await _busyBlockStream.GetBusyBlocksAsync(attendees, windowStart, windowEnd, cancellationToken);
        var cursor = windowStart;

        foreach (var block in blocks)
        {
            if (block.Start - cursor >= duration)
                return new TimeInterval(cursor, cursor + duration);
            cursor = cursor < block.End ? block.End : cursor;
            if (cursor + duration > windowEnd)
                return null;
        }

        if (windowEnd - cursor >= duration)
            return new TimeInterval(cursor, cursor + duration);
        return null;
    }
}
