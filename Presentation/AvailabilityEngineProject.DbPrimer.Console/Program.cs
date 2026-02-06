using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using AvailabilityEngineProject.Infrastructure.DbPrimer;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityEngineProject.DbPrimer.Console;

class Program
{
    static int Main(string[] args)
    {
        ParserResult<Arguments>? parseResult = Parser.Default.ParseArguments<Arguments>(args);

        if (parseResult.Errors.Any())
            return 1;

        IServiceProvider serviceProvider = ContainerConfiguration.Configure(parseResult.Value);
        IServiceScope scope = serviceProvider.CreateScope();

        try
        {
            var result = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().Upgrade();
            DisposeServices(serviceProvider);
            return (result == true) ? 0 : 1;
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Failed to upgrade database");
            DisposeServices(serviceProvider);
            return 1;
        }
    }

    private static void DisposeServices(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            return;
        }

        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
