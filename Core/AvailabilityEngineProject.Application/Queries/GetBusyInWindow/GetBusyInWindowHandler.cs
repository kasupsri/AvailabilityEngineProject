using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Queries.GetBusyInWindow;

public sealed class GetBusyInWindowHandler : IGetBusyInWindowQuery
{
    private readonly ICalendarQueryRepository _queryRepository;

    public GetBusyInWindowHandler(ICalendarQueryRepository queryRepository)
    {
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
    }

    public async Task<GetBusyInWindowResult> ExecuteAsync(GetBusyInWindowRequest request, CancellationToken cancellationToken)
    {
        if (request.Attendees.Count == 0)
            return new GetBusyInWindowResult(Array.Empty<BusyInWindowAttendee>());

        var persons = await _queryRepository.GetPersonsAsync(cancellationToken);
        var personByEmail = persons
            .Where(p => request.Attendees.Contains(p.Email, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(p => p.Email, p => p.Name, StringComparer.OrdinalIgnoreCase);
        var busyByEmail = await _queryRepository.GetBusyByEmailsAsync(request.Attendees, cancellationToken);

        var ws = request.WindowStart;
        var we = request.WindowEnd;
        var list = new List<BusyInWindowAttendee>();

        foreach (var email in request.Attendees)
        {
            var name = personByEmail.TryGetValue(email, out var n) ? n : email;
            if (!busyByEmail.TryGetValue(email, out var intervals))
            {
                list.Add(new BusyInWindowAttendee(email, name, Array.Empty<TimeInterval>()));
                continue;
            }
            var clipped = ClipToWindow(intervals, ws, we);
            list.Add(new BusyInWindowAttendee(email, name, clipped));
        }

        return new GetBusyInWindowResult(list);
    }

    private static IReadOnlyList<TimeInterval> ClipToWindow(IReadOnlyList<TimeInterval> intervals, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        var result = new List<TimeInterval>();
        foreach (var iv in intervals)
        {
            var clipStart = iv.Start < windowStart ? windowStart : iv.Start;
            var clipEnd = iv.End > windowEnd ? windowEnd : iv.End;
            if (clipEnd > clipStart)
                result.Add(new TimeInterval(clipStart, clipEnd));
        }
        return result;
    }
}
