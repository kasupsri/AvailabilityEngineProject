using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AvailabilityEngineProject.Infrastructure.Persistence.Context;

namespace AvailabilityEngineProject.Infrastructure.DbPrimer;

public class DatabaseMigrator : IDatabaseMigrator
{
    private readonly ILogger<DatabaseMigrator> _logger;
    private readonly AvailabilityEngineProjectDbContext _dbContext;
    private readonly EmbeddedScriptProviderImpl _scriptProvider;

    public DatabaseMigrator(
        ILogger<DatabaseMigrator> logger,
        AvailabilityEngineProjectDbContext dbContext,
        EmbeddedScriptProviderImpl scriptProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _scriptProvider = scriptProvider ?? throw new ArgumentNullException(nameof(scriptProvider));
    }

    public bool Upgrade()
    {
        try
        {
            _logger.LogInformation("Starting database upgrade...");

            var connection = _dbContext.Database.GetDbConnection();
            var dbPath = ExtractDatabasePath(connection.ConnectionString);
            var dbExists = File.Exists(dbPath);

            if (dbExists)
            {
                _logger.LogInformation("Database file already exists at {DbPath}. Skipping database creation.", dbPath);
            }
            else
            {
                _logger.LogInformation("Database file does not exist. Creating database at {DbPath}...", dbPath);
                _dbContext.Database.EnsureCreated();
            }

            connection.Open();

            try
            {
                var scripts = _scriptProvider.GetScripts().ToList();
                _logger.LogInformation("Found {Count} script(s) to execute", scripts.Count);

                foreach (var script in scripts)
                {
                    _logger.LogInformation("Executing script...");
                    using var command = connection.CreateCommand();
                    command.CommandText = script;
                    command.ExecuteNonQuery();
                    _logger.LogInformation("Script executed successfully");
                }

                _logger.LogInformation("Database upgrade completed successfully");
                return true;
            }
            finally
            {
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upgrade database");
            return false;
        }
    }

    private static string ExtractDatabasePath(string connectionString)
    {
        var dataSourceIndex = connectionString.IndexOf("Data Source=", StringComparison.OrdinalIgnoreCase);
        if (dataSourceIndex == -1)
            return string.Empty;

        var dataSourceValue = connectionString.Substring(dataSourceIndex + "Data Source=".Length).Trim();
        var dbPath = dataSourceValue.Split(';')[0].Trim();

        return dbPath;
    }
}
