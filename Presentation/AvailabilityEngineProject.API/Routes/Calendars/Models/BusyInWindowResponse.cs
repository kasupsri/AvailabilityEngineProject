namespace AvailabilityEngineProject.API.Routes.Calendars.Models;

public sealed record AttendeeBusyInWindowResponse(string Email, string Name, BusyIntervalResponse[] Busy);
