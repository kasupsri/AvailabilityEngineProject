using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Repository;

public interface ICalendarCommandRepository
{
    Task ReplaceBusyAsync(string email, string name, IReadOnlyList<TimeInterval> normalizedBusy, CancellationToken cancellationToken);
}
