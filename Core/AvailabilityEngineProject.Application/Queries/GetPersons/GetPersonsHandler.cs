using AvailabilityEngineProject.Application.Repository;

namespace AvailabilityEngineProject.Application.Queries.GetPersons;

public sealed class GetPersonsHandler : IGetPersonsQuery
{
    private readonly ICalendarQueryRepository _queryRepository;

    public GetPersonsHandler(ICalendarQueryRepository queryRepository)
    {
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
    }

    public Task<IReadOnlyList<PersonInfo>> ExecuteAsync(CancellationToken cancellationToken) =>
        _queryRepository.GetPersonsAsync(cancellationToken);
}
