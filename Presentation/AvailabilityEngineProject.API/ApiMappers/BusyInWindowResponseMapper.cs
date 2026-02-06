using System.Globalization;
using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Queries.GetBusyInWindow;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.API.ApiMappers;

public static class BusyInWindowResponseMapper
{
    public static AttendeeBusyInWindowResponse ToResponse(BusyInWindowAttendee attendee) =>
        new AttendeeBusyInWindowResponse(
            attendee.Email,
            attendee.Name,
            attendee.Busy.Select(i => new BusyIntervalResponse(
                i.Start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                i.End.UtcDateTime.ToString("o", CultureInfo.InvariantCulture))).ToArray());

    public static AttendeeBusyInWindowResponse[] ToResponseArray(GetBusyInWindowResult result) =>
        result.Attendees.Select(ToResponse).ToArray();
}
