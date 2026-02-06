using System.Globalization;
using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Commands.PutBusy;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.API.ApiMappers;

public static class BusyPutResponseMapper
{
    public static BusyPutResponse ToResponse(PutBusyResult result) =>
        new BusyPutResponse(
            result.Email,
            result.Name,
            result.Busy.Select(i => new BusyIntervalResponse(
                i.Start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                i.End.UtcDateTime.ToString("o", CultureInfo.InvariantCulture))).ToArray());
}
