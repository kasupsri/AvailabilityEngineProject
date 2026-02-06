using Microsoft.Extensions.Hosting;

namespace AvailabilityEngineProject.API.Extensions.Setup;

public static class HostConfigurationExtensions
{
    public static IHostBuilder AddConfigurations(this IHostBuilder host)
    {
        host.ConfigureAppConfiguration((context, config) =>
        {
            var env = context.HostingEnvironment;
            config.AddConfigurationFiles(env);
        });

        return host;
    }

    public static IConfigurationBuilder AddConfigurationFiles(this IConfigurationBuilder config, IHostEnvironment env)
    {
        const string configurationsDirectory = "Configurations";

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)

              .AddJsonFile($"{configurationsDirectory}/Logger.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"{configurationsDirectory}/Logger.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)

              .AddJsonFile($"{configurationsDirectory}/DatabaseConfiguration.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"{configurationsDirectory}/DatabaseConfiguration.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)

              .AddJsonFile($"{configurationsDirectory}/Observability.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"{configurationsDirectory}/Observability.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)

              .AddJsonFile($"{configurationsDirectory}/ApiConfiguration.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"{configurationsDirectory}/ApiConfiguration.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)

              .AddEnvironmentVariables();

        return config;
    }
}
