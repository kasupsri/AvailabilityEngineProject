using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.API.ApiMappers;

public static class PersonResponseMapper
{
    public static PersonResponse ToResponse(Person person) =>
        new PersonResponse(person.Email, person.Name);
}
