namespace AvailabilityEngineProject.API.Routes.Calendars.Models;

public sealed record BusyIntervalResponse(string Start, string End);

public sealed record BusyPutResponse(string Email, string Name, BusyIntervalResponse[] Busy);
