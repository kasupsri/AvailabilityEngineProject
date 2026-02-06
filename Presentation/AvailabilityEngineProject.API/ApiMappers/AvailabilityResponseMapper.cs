using System.Globalization;
using AvailabilityEngineProject.API.Routes.Availability.Models;
using AvailabilityEngineProject.Application.Queries.GetAvailability;

namespace AvailabilityEngineProject.API.ApiMappers;

public static class AvailabilityResponseMapper
{
    public static AvailabilityResponse ToResponse(GetAvailabilityResult result)
    {
        if (!result.Found || result.Slot is null)
            return new AvailabilityResponse(false);

        var slot = result.Slot;
        return new AvailabilityResponse(
            true,
            slot.Start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
            slot.End.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
    }
}
