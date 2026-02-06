using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Commands.PutBusy;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.API.Routes.Calendars.Endpoints;

public static class PutBusy
{
    public static async Task<IResult> Handle(
        [FromRoute] string email,
        [FromBody] BusyPutRequest request,
        IPutBusyCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest("email is required");
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("name is required");

        var intervals = new List<TimeInterval>();
        foreach (var b in request.Busy ?? Array.Empty<BusyIntervalDto>())
        {
            if (!DateTimeOffset.TryParse(b.Start, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var start) ||
                !DateTimeOffset.TryParse(b.End, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var end))
            {
                return Results.BadRequest($"Invalid interval: start='{b.Start}', end='{b.End}'. Use ISO-8601 UTC.");
            }
            if (end <= start)
                return Results.BadRequest($"Invalid interval: end must be after start (start='{b.Start}', end='{b.End}').");
            intervals.Add(new TimeInterval(start, end));
        }

        try
        {
            var result = await command.ExecuteAsync(email, request.Name, intervals, cancellationToken);
            var response = new BusyPutResponse(
                result.Email,
                result.Name,
                result.Busy.Select(i => new BusyIntervalResponse(
                    i.Start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                    i.End.UtcDateTime.ToString("o", CultureInfo.InvariantCulture))).ToArray());
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
