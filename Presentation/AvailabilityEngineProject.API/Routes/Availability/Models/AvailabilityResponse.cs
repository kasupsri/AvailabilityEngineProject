namespace AvailabilityEngineProject.API.Routes.Availability.Models;

public sealed record AvailabilityResponse(bool Found, string? Start = null, string? End = null);
