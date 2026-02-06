using AvailabilityEngineProject.API.Routes.Availability;
using AvailabilityEngineProject.API.Routes.Calendars;
using Kasupsri.Utilities.Logging;
using Kasupsri.Utilities.Observability;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AvailabilityEngineProject.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureApplication(this WebApplication app,
                                                     IWebHostEnvironment env,
                                                     IConfiguration configuration)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calendar Availability API V1");
        });

        app.UseRequestSessionLogging();
        app.UseSerilogRequestLogging();
        app.UseCors();
        app.UseRouting();

        app.MapCalendarGroup("/api");
        app.MapAvailabilityGroup("/api");

        app.MapOpenTelemetryEndPoints(configuration);
        app.MapHealthChecks("/health");

        return app;
    }
}
