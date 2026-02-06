using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Domain;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;
using AvailabilityEngineProject.Infrastructure.Persistence.Entity;
using AvailabilityEngineProject.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityEngineProject.Infrastructure.Persistence.Command;

public sealed class CalendarCommandRepository : ICalendarCommandRepository
{
    private readonly AvailabilityEngineProjectDbContext _context;

    public CalendarCommandRepository(AvailabilityEngineProjectDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task ReplaceBusyAsync(string email, string name, IReadOnlyList<TimeInterval> normalizedBusy, CancellationToken cancellationToken)
    {
        var person = await _context.Persons.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        if (person == null)
        {
            person = new Person { Email = email, Name = name };
            _context.Persons.Add(person);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            person.Name = name;
        }

        var existing = await _context.PersonBusyIntervals
            .Where(x => x.PersonId == person.Id)
            .ToListAsync(cancellationToken);
        _context.PersonBusyIntervals.RemoveRange(existing);

        var entities = normalizedBusy
            .Select(i => PersonBusyIntervalMapper.ToEntity(person.Id, i))
            .ToList();
        await _context.PersonBusyIntervals.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
