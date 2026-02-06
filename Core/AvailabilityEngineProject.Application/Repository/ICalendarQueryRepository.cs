using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Repository;

public interface ICalendarQueryRepository
{
    Task<IReadOnlyDictionary<string, IReadOnlyList<TimeInterval>>> GetBusyByEmailsAsync(IReadOnlyList<string> emails, CancellationToken cancellationToken);
    Task<IReadOnlyList<Person>> GetPersonsAsync(CancellationToken cancellationToken);
}
