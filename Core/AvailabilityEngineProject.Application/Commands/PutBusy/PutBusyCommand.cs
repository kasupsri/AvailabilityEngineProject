using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Application.Services;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Commands.PutBusy;

public sealed class PutBusyCommand : IPutBusyCommand
{
    private readonly ICalendarCommandRepository _commandRepository;
    private readonly ICalendarQueryRepository _queryRepository;
    private readonly IBusyNormalizationService _normalizationService;

    public PutBusyCommand(
        ICalendarCommandRepository commandRepository,
        ICalendarQueryRepository queryRepository,
        IBusyNormalizationService normalizationService)
    {
        _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
        _normalizationService = normalizationService ?? throw new ArgumentNullException(nameof(normalizationService));
    }

    public async Task<PutBusyResult> ExecuteAsync(string email, string name, IReadOnlyList<TimeInterval> busy, CancellationToken cancellationToken)
    {
        var existingByEmail = await _queryRepository.GetBusyByEmailsAsync(new[] { email }, cancellationToken);
        var existing = existingByEmail.TryGetValue(email, out var list) ? list : Array.Empty<TimeInterval>();
        var combined = existing.Concat(busy).ToList();
        var normalized = _normalizationService.Normalize(combined);
        await _commandRepository.ReplaceBusyAsync(email, name, normalized, cancellationToken);
        return new PutBusyResult(email, name, normalized);
    }
}
