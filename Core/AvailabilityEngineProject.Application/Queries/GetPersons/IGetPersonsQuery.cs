using AvailabilityEngineProject.Application.Repository;

namespace AvailabilityEngineProject.Application.Queries.GetPersons;

public interface IGetPersonsQuery
{
    Task<IReadOnlyList<PersonInfo>> ExecuteAsync(CancellationToken cancellationToken);
}
