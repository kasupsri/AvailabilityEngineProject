using Autofac;
using AvailabilityEngineProject.Application.Commands.PutBusy;
using AvailabilityEngineProject.Application.Queries.GetAvailability;
using AvailabilityEngineProject.Application.Queries.GetPersons;
using AvailabilityEngineProject.Application.Repository;
using AvailabilityEngineProject.Application.Services;
using AvailabilityEngineProject.Application.Strategies;

namespace AvailabilityEngineProject.Application;

public class ApplicationRegisterModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<BusyNormalizationService>()
            .As<IBusyNormalizationService>()
            .SingleInstance();

        builder.RegisterType<UnionAllBusyStream>()
            .As<IBusyBlockStream>()
            .InstancePerLifetimeScope();

        builder.RegisterType<SystemClock>()
            .As<IClock>()
            .SingleInstance();

        builder.RegisterType<AvailabilityService>()
            .As<IAvailabilityService>()
            .InstancePerLifetimeScope();

        builder.RegisterType<PutBusyCommand>()
            .As<IPutBusyCommand>()
            .InstancePerLifetimeScope();

        builder.RegisterType<GetAvailabilityHandler>()
            .As<IGetAvailabilityQuery>()
            .InstancePerLifetimeScope();

        builder.RegisterType<GetPersonsHandler>()
            .As<IGetPersonsQuery>()
            .InstancePerLifetimeScope();
    }
}
