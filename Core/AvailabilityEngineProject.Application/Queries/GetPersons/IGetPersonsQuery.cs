using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Queries.GetPersons;

public interface IGetPersonsQuery
{
    Task<IReadOnlyList<Person>> ExecuteAsync(CancellationToken cancellationToken);
}
