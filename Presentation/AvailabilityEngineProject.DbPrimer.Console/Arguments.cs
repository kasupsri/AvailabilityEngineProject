using CommandLine;

namespace AvailabilityEngineProject.DbPrimer.Console;

public class Arguments
{
    [Option('c', "connection-string", Required = false, HelpText = "SQLite connection string. Default: Data Source=data/app.db")]
    public string? ConnectionString { get; set; }
}
