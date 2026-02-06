using System.Diagnostics.CodeAnalysis;
using AvailabilityEngineProject.API.Routes.Calendars.Endpoints;
using AvailabilityEngineProject.API.Routes.Calendars.Models;

namespace AvailabilityEngineProject.API.Routes.Calendars;

public static class CalendarModule
{
    public static RouteGroupBuilder MapCalendarGroup(this IEndpointRouteBuilder app, [StringSyntax("Route")] string prefix)
    {
        return app.MapGroup($"{prefix}/calendars")
            .MapCalendarEndpoints()
            .WithTags("Calendars");
    }

    private static RouteGroupBuilder MapCalendarEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/persons", GetPersons.Handle)
            .WithSummary("List all persons (email and name)")
            .Produces<PersonResponse[]>(200);

        group.MapPut("/{email}/busy", PutBusy.Handle)
            .WithSummary("Merge busy intervals for a person (adds to existing, normalized). Name is required.")
            .Accepts<BusyPutRequest>("application/json")
            .Produces<BusyPutResponse>(200)
            .Produces(400);

        group.MapGet("/busy", GetBusyInWindow.Handle)
            .WithSummary("Get busy intervals per attendee in a time window.")
            .Produces<AttendeeBusyInWindowResponse[]>(200)
            .Produces(400);
        return group;
    }
}
