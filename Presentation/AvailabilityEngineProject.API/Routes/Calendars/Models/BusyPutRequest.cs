namespace AvailabilityEngineProject.API.Routes.Calendars.Models;

public sealed record BusyIntervalDto(string Start, string End);

public sealed record BusyPutRequest(string Name, BusyIntervalDto[] Busy);
