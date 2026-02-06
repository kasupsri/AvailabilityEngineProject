using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Queries.GetPersons;

namespace AvailabilityEngineProject.API.Routes.Calendars.Endpoints;

public static class GetPersons
{
    public static async Task<IResult> Handle(IGetPersonsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var persons = await query.ExecuteAsync(cancellationToken);
            var response = persons.Select(p => new PersonResponse(p.Email, p.Name)).ToArray();
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
