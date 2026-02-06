using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Repository;

public interface ICalendarQueryRepository
{
    Task<IReadOnlyDictionary<string, IReadOnlyList<TimeInterval>>> GetBusyByEmailsAsync(IReadOnlyList<string> emails, CancellationToken cancellationToken);
    Task<IReadOnlyList<PersonInfo>> GetPersonsAsync(CancellationToken cancellationToken);
}

public sealed record PersonInfo(string Email, string Name);
