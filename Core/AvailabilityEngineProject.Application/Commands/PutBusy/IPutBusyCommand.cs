using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Commands.PutBusy;

public interface IPutBusyCommand
{
    Task<PutBusyResult> ExecuteAsync(string email, string name, IReadOnlyList<TimeInterval> busy, CancellationToken cancellationToken);
}

public sealed record PutBusyResult(string Email, string Name, IReadOnlyList<TimeInterval> Busy);
