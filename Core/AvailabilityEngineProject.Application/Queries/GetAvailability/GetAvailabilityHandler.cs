using AvailabilityEngineProject.Application.Services;
using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Queries.GetAvailability;

public sealed class GetAvailabilityHandler : IGetAvailabilityQuery
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IClock _clock;

    public GetAvailabilityHandler(IAvailabilityService availabilityService, IClock clock)
    {
        _availabilityService = availabilityService ?? throw new ArgumentNullException(nameof(availabilityService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<GetAvailabilityResult> ExecuteAsync(GetAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var duration = TimeSpan.FromMinutes(request.DurationMinutes);
        var now = _clock.GetUtcNow();
        var effectiveStart = request.WindowStart < now ? now : request.WindowStart;

        if (effectiveStart + duration > request.WindowEnd)
            return new GetAvailabilityResult(false, null);

        if (request.Attendees.Count == 0)
            return new GetAvailabilityResult(true, new TimeInterval(effectiveStart, effectiveStart + duration));

        var slot = await _availabilityService.FindEarliestSlotAsync(
            request.Attendees,
            effectiveStart,
            request.WindowEnd,
            duration,
            cancellationToken);

        return slot is null
            ? new GetAvailabilityResult(false, null)
            : new GetAvailabilityResult(true, slot);
    }
}
