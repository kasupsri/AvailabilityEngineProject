using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using AvailabilityEngineProject.API.ApiMappers;
using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Queries.GetBusyInWindow;

namespace AvailabilityEngineProject.API.Routes.Calendars.Endpoints;

public static class GetBusyInWindow
{
    public static async Task<IResult> Handle(
        [FromQuery] string attendees,
        [FromQuery] string windowStart,
        [FromQuery] string windowEnd,
        IGetBusyInWindowQuery query,
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

        try
        {
            var request = new GetBusyInWindowRequest(attendeeList, ws, we);
            var result = await query.ExecuteAsync(request, cancellationToken);
            return Results.Ok(BusyInWindowResponseMapper.ToResponseArray(result));
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
