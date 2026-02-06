using Autofac;
using Microsoft.EntityFrameworkCore;
using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Infrastructure.Persistence.Command;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;
using AvailabilityEngineProject.Infrastructure.Persistence.Query;

namespace AvailabilityEngineProject.Infrastructure.Persistence;

public class PersistenceRegisterModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<AvailabilityEngineProjectDbContext>()
            .AsSelf()
            .As<DbContext>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CalendarCommandRepository>()
            .As<ICalendarCommandRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CalendarQueryRepository>()
            .As<ICalendarQueryRepository>()
            .InstancePerLifetimeScope();
    }
}
