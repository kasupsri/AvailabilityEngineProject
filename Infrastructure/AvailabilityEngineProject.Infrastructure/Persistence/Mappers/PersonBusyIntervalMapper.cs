using AvailabilityEngineProject.Domain;
using PersonBusyIntervalEntity = AvailabilityEngineProject.Infrastructure.Persistence.Entity.PersonBusyInterval;

namespace AvailabilityEngineProject.Infrastructure.Persistence.Mappers;

public static class PersonBusyIntervalMapper
{
    public static TimeInterval ToTimeInterval(PersonBusyIntervalEntity entity) =>
        new TimeInterval(entity.StartUtc, entity.EndUtc);

    public static PersonBusyIntervalEntity ToEntity(int personId, TimeInterval interval) =>
        new PersonBusyIntervalEntity
        {
            PersonId = personId,
            StartUtc = interval.Start,
            EndUtc = interval.End
        };
}
