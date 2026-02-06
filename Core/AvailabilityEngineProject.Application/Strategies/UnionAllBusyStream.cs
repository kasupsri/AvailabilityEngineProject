using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Application.Services;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Strategies;

public sealed class UnionAllBusyStream : IBusyBlockStream
{
    private readonly ICalendarQueryRepository _queryRepository;

    public UnionAllBusyStream(ICalendarQueryRepository queryRepository)
    {
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
    }

    public async Task<IReadOnlyList<TimeInterval>> GetBusyBlocksAsync(
        IReadOnlyList<string> attendees,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken)
    {
        if (attendees.Count == 0)
            return Array.Empty<TimeInterval>();

        var busyByPerson = await _queryRepository.GetBusyByEmailsAsync(attendees, cancellationToken);
        var allInWindow = new List<TimeInterval>();

        foreach (var email in attendees)
        {
            if (!busyByPerson.TryGetValue(email, out var intervals))
                continue;
            foreach (var interval in intervals)
            {
                var clipStart = interval.Start < windowStart ? windowStart : interval.Start;
                var clipEnd = interval.End > windowEnd ? windowEnd : interval.End;
                if (clipEnd > clipStart)
                    allInWindow.Add(new TimeInterval(clipStart, clipEnd));
            }
        }

        if (allInWindow.Count == 0)
            return Array.Empty<TimeInterval>();

        var normalizer = new BusyNormalizationService();
        return normalizer.Normalize(allInWindow);
    }
}
