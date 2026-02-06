using Serilog;
using AvailabilityEngineProject.API.Extensions;
using AvailabilityEngineProject.API.Extensions.Setup;

Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .MinimumLevel.Information()
             .CreateBootstrapLogger();

var builder = WebApplication
              .CreateBuilder(args);

builder.ConfigureApplicationBuilder();

var apiBaseUrl = builder.Configuration.GetValue<string>("urls");
if (!string.IsNullOrEmpty(apiBaseUrl))
{
    builder.WebHost.UseUrls(apiBaseUrl);
    Log.Information("Server configured to start on {BaseUrl}", apiBaseUrl);
}
else
{
    Log.Warning("No 'urls' configuration found. Using default port.");
}

var app = builder
          .Build()
          .ConfigureApplication(builder.Environment, builder.Configuration);

try
{
    Log.Information("Server About To Start on {BaseUrl}", apiBaseUrl ?? "default");
    await app.RunAsync(CancellationToken.None);
    return 0;
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Web Server terminated unexpectedly");
    return 1;
}
finally
{
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
