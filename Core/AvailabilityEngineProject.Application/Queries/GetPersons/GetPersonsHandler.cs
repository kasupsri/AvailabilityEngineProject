using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Queries.GetPersons;

public sealed class GetPersonsHandler : IGetPersonsQuery
{
    private readonly ICalendarQueryRepository _queryRepository;

    public GetPersonsHandler(ICalendarQueryRepository queryRepository)
    {
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
    }

    public Task<IReadOnlyList<Person>> ExecuteAsync(CancellationToken cancellationToken) =>
        _queryRepository.GetPersonsAsync(cancellationToken);
}
