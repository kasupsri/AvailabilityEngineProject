using Autofac;
using Autofac.Extensions.DependencyInjection;
using Kasupsri.Utilities.Logging;
using Kasupsri.Utilities.Observability;
using Microsoft.EntityFrameworkCore;
using Serilog;
using AvailabilityEngineProject.API.Extensions.Setup;
using AvailabilityEngineProject.Application;
using AvailabilityEngineProject.Infrastructure.Persistence;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;

namespace AvailabilityEngineProject.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureApplicationBuilder(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddConfigurationFiles(builder.Environment);

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        
        builder.Host.ConfigureContainer<ContainerBuilder>(container =>
        {
            container.RegisterModule(new PersistenceRegisterModule());
            container.RegisterModule(new ApplicationRegisterModule());
        });

        var configConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var rawConnectionString = string.IsNullOrWhiteSpace(configConnectionString)
            ? DatabasePathHelper.GetConnectionString()
            : configConnectionString;
        
        var dbPath = ExtractDatabasePath(rawConnectionString);
        DatabasePathHelper.EnsureDirectoryExists(dbPath);
        
        var connectionString = $"Data Source={dbPath}";
        
        builder.Services.AddDbContext<AvailabilityEngineProjectDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Calendar Availability API",
                Version = "v1",
                Description = "API for calendar busy intervals and availability"
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.Services.AddHealthChecks()
               .AddDbContextCheck<AvailabilityEngineProjectDbContext>();

        builder.Services.AddRequestSessionLogging();

        builder.Services.ConfigureOpenTelemetry(builder.Configuration);

        builder.Host.AddSerilog();

        return builder;
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
