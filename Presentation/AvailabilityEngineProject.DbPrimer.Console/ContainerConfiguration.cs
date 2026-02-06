using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using AvailabilityEngineProject.Infrastructure.DbPrimer;
using AvailabilityEngineProject.Infrastructure.Persistence;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityEngineProject.DbPrimer.Console;

internal static class ContainerConfiguration
{
    public static IServiceProvider Configure(Arguments arguments)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile(path: "appsettings.json",
                         optional: false,
                         reloadOnChange: true);

        IConfigurationRoot configuration = builder.Build();

        Log.Logger = new LoggerConfiguration()
                     .ReadFrom.Configuration(configuration)
                     .CreateLogger();

        var serviceCollection = new ServiceCollection();

        var configConnectionString = configuration.GetConnectionString("DefaultConnection");
        var rawConnectionString = arguments.ConnectionString 
            ?? (string.IsNullOrWhiteSpace(configConnectionString) 
                ? DatabasePathHelper.GetConnectionString() 
                : configConnectionString);

        var dbPath = ExtractDatabasePath(rawConnectionString);
        DatabasePathHelper.EnsureDirectoryExists(dbPath);
        
        var connectionString = $"Data Source={dbPath}";

        serviceCollection.AddDbContext<AvailabilityEngineProjectDbContext>(options =>
            options.UseSqlite(connectionString));

        serviceCollection.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        serviceCollection.AddScoped<EmbeddedScriptProviderImpl>();
        serviceCollection.AddScoped<Arguments>(_ => arguments);

        serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(serviceCollection);

        return new AutofacServiceProvider(containerBuilder.Build());
    }

    private static string ExtractDatabasePath(string connectionString)
    {
        var dataSourceIndex = connectionString.IndexOf("Data Source=", StringComparison.OrdinalIgnoreCase);
        if (dataSourceIndex == -1)
            return DatabasePathHelper.GetDatabasePath();

        var dataSourceValue = connectionString.Substring(dataSourceIndex + "Data Source=".Length).Trim();
        var dbPath = dataSourceValue.Split(';')[0].Trim();

        if (string.IsNullOrEmpty(dbPath))
            return DatabasePathHelper.GetDatabasePath();

        if (Path.IsPathRooted(dbPath))
            return dbPath;

        var solutionRoot = DatabasePathHelper.GetSolutionRoot();
        return Path.GetFullPath(Path.Combine(solutionRoot, dbPath));
    }
}
