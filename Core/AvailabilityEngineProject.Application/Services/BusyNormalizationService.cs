using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Services;

public interface IBusyNormalizationService
{
    IReadOnlyList<TimeInterval> Normalize(IEnumerable<TimeInterval> intervals);
}

public sealed class BusyNormalizationService : IBusyNormalizationService
{
    public IReadOnlyList<TimeInterval> Normalize(IEnumerable<TimeInterval> intervals)
    {
        var valid = intervals.Where(i => i.IsValid).ToList();
        if (valid.Count == 0)
            return Array.Empty<TimeInterval>();

        var ordered = valid.OrderBy(i => i.Start).ThenBy(i => i.End).ToList();
        var merged = new List<TimeInterval>();
        var current = ordered[0];

        for (var i = 1; i < ordered.Count; i++)
        {
            var next = ordered[i];
            if (next.Start < current.End)
                current = current with { End = current.End > next.End ? current.End : next.End };
            else
            {
                merged.Add(current);
                current = next;
            }
        }
        merged.Add(current);
        return merged;
    }
}
