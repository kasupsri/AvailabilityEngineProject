using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Kasupsri.Utilities.Observability;

public static class OpenTelemetryExtensions
{
    private static ResourceBuilder? _resource = null;

    private static bool IsObservabilityEnabled(IConfiguration configuration)
    {
        return configuration.GetSection(nameof(ObservabilityConfiguration))
                            .Get<ObservabilityConfiguration>()?
                            .Enabled
               ?? false;
    }

    private static string? SamplerEnabled(IConfiguration configuration)
    {
        return configuration.GetSection(nameof(ObservabilityConfiguration))
                            .Get<ObservabilityConfiguration>()?
                            .Sampler;
    }

    static ResourceBuilder GetResourceBuilder()
    {
        ResourceBuilder BuildBuilder()
        {
            var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;

            var buildVersion = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "1.0.0";

            return ResourceBuilder.CreateDefault()
                                  .AddService(serviceName: assemblyName ?? "unknownService",
                                              serviceVersion: buildVersion);
        }

        return _resource ??= BuildBuilder();
    }

    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services,
                                                            IConfiguration configuration,
                                                            string[]? tracingCustomSources = null,
                                                            string[]? meterCustomSources = null)
    {
        if (!IsObservabilityEnabled(configuration))
        {
            return services;
        }

        services.AddOpenTelemetry()
                .WithMetrics(x =>
                {
                    x.SetResourceBuilder(GetResourceBuilder())
                     .AddRuntimeInstrumentation()
                     .AddAspNetCoreInstrumentation()
                     .AddHttpClientInstrumentation()
                     .AddMeter("Microsoft.AspNetCore.Hosting",
                               "Microsoft-AspNetCore-Server-Kestrel",
                               "Microsoft.AspNetCore.Server.Kestrel",
                               "System.Net.Http",
                               "System.Net.Sockets");
                    
                    if (meterCustomSources != null && meterCustomSources.Length != 0)
                    {
                        x.AddMeter(meterCustomSources);
                    }
                })
                .WithTracing(x =>
                {
                    switch (SamplerEnabled(configuration))
                    {
                        case nameof(AlwaysOnSampler):
                        {
                            x.SetSampler<AlwaysOnSampler>();
                            break;
                        }
                        case nameof(AlwaysOffSampler):
                        {
                            x.SetSampler<AlwaysOffSampler>();
                            break;
                        }
                    }

                    x.SetResourceBuilder(GetResourceBuilder())
                     .AddAspNetCoreInstrumentation(options =>
                     {
                         options.EnrichWithHttpRequest = (activity, request) =>
                         {
                             activity.SetTag("requestProtocol", request.Protocol);
                         };
                         options.EnrichWithHttpResponse = (activity, response) =>
                         {
                             activity.SetTag("responseLength", response.ContentLength);
                         };
                         options.RecordException = true;
                         options.EnrichWithException = (activity, exception) =>
                         {
                             activity.SetTag("exceptionType", exception.GetType().ToString());
                             activity.SetTag("stackTrace", exception.StackTrace);
                         };
                     })
                     .AddHttpClientInstrumentation(options =>
                     {
                         options.RecordException = true;
                         options.EnrichWithException = (activity, exception) =>
                         {
                             activity.SetTag("exceptionType", exception.GetType().ToString());
                             activity.SetTag("stackTrace", exception.StackTrace);
                         };
                     })
                     .AddEntityFrameworkCoreInstrumentation(options =>
                     {
                         options.SetDbStatementForText = true;
                         options.SetDbStatementForStoredProcedure = true;

                         options.EnrichWithIDbCommand = (activity, command) => { };
                     });

                    if (tracingCustomSources != null && tracingCustomSources.Length != 0)
                    {
                        x.AddSource(tracingCustomSources);
                    }
                });

        services.AddOpenTelemetryExporters(configuration);

        return services;
    }

    private static IServiceCollection AddOpenTelemetryExporters(this IServiceCollection services,
                                                                IConfiguration configuration)
    {
        services.Configure<OpenTelemetryLoggerOptions>(logging =>
        {
            logging.IncludeScopes = true;
            logging.IncludeFormattedMessage = true;

            logging.SetResourceBuilder(GetResourceBuilder());
            logging.AddOtlpExporter();
        });

        services.ConfigureOpenTelemetryMeterProvider(metrics
                                                         => metrics.AddOtlpExporter());

        services.ConfigureOpenTelemetryTracerProvider(tracer => tracer.AddOtlpExporter());

        return services;
    }

    public static IEndpointRouteBuilder MapOpenTelemetryEndPoints(this IEndpointRouteBuilder routeBuilder, IConfiguration configuration)
    {
        if (!IsObservabilityEnabled(configuration))
        {
            return routeBuilder;
        }
        
        return routeBuilder;
    }
}
