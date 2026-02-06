using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Kasupsri.Utilities.Logging.Correlation;
using Serilog;

namespace Kasupsri.Utilities.Logging;

public static class LoggingExtensions
{
    public static IHostBuilder AddSerilog(this IHostBuilder host)
    {
        host.UseSerilog((context, services, configuration) =>

                configuration.ReadFrom.Configuration(context.Configuration)
                             .ReadFrom.Services(services)
                             .Enrich.FromLogContext());

        return host;
    }

    public static IServiceCollection AddRequestSessionLogging(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();

        return services;
    }

    public static IApplicationBuilder UseRequestSessionLogging(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseMiddleware<CorrelationHeaderMiddleware>();

        return applicationBuilder;
    }
}
