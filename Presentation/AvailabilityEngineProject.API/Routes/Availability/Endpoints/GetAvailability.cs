using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using AvailabilityEngineProject.API.Routes.Availability.Models;
using AvailabilityEngineProject.Application.Queries.GetAvailability;

namespace AvailabilityEngineProject.API.Routes.Availability.Endpoints;

public static class GetAvailability
{
    public static async Task<IResult> Handle(
        [FromQuery] string attendees,
        [FromQuery] string windowStart,
        [FromQuery] string windowEnd,
        [FromQuery] int durationMinutes,
        IGetAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(attendees))
            return Results.BadRequest("attendees is required");
        if (string.IsNullOrWhiteSpace(windowStart))
            return Results.BadRequest("windowStart is required");
        if (string.IsNullOrWhiteSpace(windowEnd))
            return Results.BadRequest("windowEnd is required");
        if (durationMinutes <= 0)
            return Results.BadRequest("durationMinutes must be positive");

        if (!DateTimeOffset.TryParse(windowStart, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var ws) ||
            !DateTimeOffset.TryParse(windowEnd, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var we))
        {
            return Results.BadRequest("windowStart and windowEnd must be valid ISO-8601 UTC.");
        }
        if (we <= ws)
            return Results.BadRequest("windowEnd must be after windowStart.");

        var attendeeList = attendees.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var request = new GetAvailabilityRequest(attendeeList, ws, we, durationMinutes);

        try
        {
            var result = await query.ExecuteAsync(request, cancellationToken);
            if (!result.Found || result.Slot is null)
                return Results.Ok(new AvailabilityResponse(false));

            var slot = result.Slot;
            return Results.Ok(new AvailabilityResponse(
                true,
                slot.Start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                slot.End.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)));
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
