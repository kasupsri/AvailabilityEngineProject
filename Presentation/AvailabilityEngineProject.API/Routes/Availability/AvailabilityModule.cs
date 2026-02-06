using System.Diagnostics.CodeAnalysis;
using AvailabilityEngineProject.API.Routes.Availability.Endpoints;
using AvailabilityEngineProject.API.Routes.Availability.Models;

namespace AvailabilityEngineProject.API.Routes.Availability;

public static class AvailabilityModule
{
    public static RouteGroupBuilder MapAvailabilityGroup(this IEndpointRouteBuilder app, [StringSyntax("Route")] string prefix)
    {
        return app.MapGroup($"{prefix}/availability")
            .MapAvailabilityEndpoints()
            .WithTags("Availability");
    }

    private static RouteGroupBuilder MapAvailabilityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAvailability.Handle)
            .WithSummary("Get earliest available slot for attendees in window")
            .Produces<AvailabilityResponse>(200);
        return group;
    }
}
