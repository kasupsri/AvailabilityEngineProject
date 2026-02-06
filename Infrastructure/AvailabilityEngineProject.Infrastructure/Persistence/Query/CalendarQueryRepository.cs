using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;
using AvailabilityEngineProject.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;
using PersonEntity = AvailabilityEngineProject.Infrastructure.Persistence.Entity.Person;
using PersonDomain = AvailabilityEngineProject.Domain.Person;
using TimeInterval = AvailabilityEngineProject.Domain.TimeInterval;

namespace AvailabilityEngineProject.Infrastructure.Persistence.Query;

public sealed class CalendarQueryRepository : ICalendarQueryRepository
{
    private readonly AvailabilityEngineProjectDbContext _context;

    public CalendarQueryRepository(AvailabilityEngineProjectDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<TimeInterval>>> GetBusyByEmailsAsync(IReadOnlyList<string> emails, CancellationToken cancellationToken)
    {
        if (emails.Count == 0)
            return new Dictionary<string, IReadOnlyList<TimeInterval>>();

        var persons = await _context.Persons
            .Where(p => emails.Contains(p.Email))
            .ToListAsync(cancellationToken);
        var personIds = persons.Select(p => p.Id).ToList();
        var idToEmail = persons.ToDictionary(p => p.Id, p => p.Email);

        var result = new Dictionary<string, IReadOnlyList<TimeInterval>>(StringComparer.OrdinalIgnoreCase);
        foreach (var email in emails)
        {
            result[email] = Array.Empty<TimeInterval>();
        }

        if (personIds.Count == 0)
            return result;

        var list = await _context.PersonBusyIntervals
            .Where(x => personIds.Contains(x.PersonId))
            .ToListAsync(cancellationToken);

        foreach (var g in list.OrderBy(x => x.PersonId).ThenBy(x => x.StartUtc).GroupBy(x => x.PersonId))
        {
            if (idToEmail.TryGetValue(g.Key, out var email))
                result[email] = g.Select(PersonBusyIntervalMapper.ToTimeInterval).ToList();
        }
        return result;
    }

    public async Task<IReadOnlyList<AvailabilityEngineProject.Domain.Person>> GetPersonsAsync(CancellationToken cancellationToken)
    {
        var list = await _context.Persons
            .OrderBy(p => p.Email)
            .Select(p => new PersonDomain(p.Id, p.Email, p.Name))
            .ToListAsync(cancellationToken);
        return list;
    }
}
