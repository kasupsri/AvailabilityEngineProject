using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.API.Routes.Calendars.Endpoints;

public static class GetBusyInWindow
{
    public static async Task<IResult> Handle(
        [FromQuery] string attendees,
        [FromQuery] string windowStart,
        [FromQuery] string windowEnd,
        ICalendarQueryRepository queryRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(attendees))
            return Results.Ok(Array.Empty<AttendeeBusyInWindowResponse>());
        if (string.IsNullOrWhiteSpace(windowStart) || string.IsNullOrWhiteSpace(windowEnd))
            return Results.BadRequest("windowStart and windowEnd are required.");

        if (!DateTimeOffset.TryParse(windowStart, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var ws) ||
            !DateTimeOffset.TryParse(windowEnd, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var we))
        {
            return Results.BadRequest("windowStart and windowEnd must be valid ISO-8601 UTC.");
        }
        if (we <= ws)
            return Results.BadRequest("windowEnd must be after windowStart.");

        var attendeeList = attendees.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (attendeeList.Count == 0)
            return Results.Ok(Array.Empty<AttendeeBusyInWindowResponse>());

        var persons = await queryRepository.GetPersonsAsync(cancellationToken);
        var personByEmail = persons.Where(p => attendeeList.Contains(p.Email, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(p => p.Email, p => p.Name, StringComparer.OrdinalIgnoreCase);
        var busyByEmail = await queryRepository.GetBusyByEmailsAsync(attendeeList, cancellationToken);

        var result = new List<AttendeeBusyInWindowResponse>();
        foreach (var email in attendeeList)
        {
            var name = personByEmail.TryGetValue(email, out var n) ? n : email;
            if (!busyByEmail.TryGetValue(email, out var intervals))
            {
                result.Add(new AttendeeBusyInWindowResponse(email, name, Array.Empty<BusyIntervalResponse>()));
                continue;
            }
            var clipped = new List<BusyIntervalResponse>();
            foreach (var iv in intervals)
            {
                var clipStart = iv.Start < ws ? ws : iv.Start;
                var clipEnd = iv.End > we ? we : iv.End;
                if (clipEnd > clipStart)
                    clipped.Add(new BusyIntervalResponse(
                        clipStart.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                        clipEnd.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)));
            }
            result.Add(new AttendeeBusyInWindowResponse(email, name, clipped.ToArray()));
        }
        return Results.Ok(result);
    }
}
