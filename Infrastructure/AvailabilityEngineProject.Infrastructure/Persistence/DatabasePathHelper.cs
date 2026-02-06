namespace AvailabilityEngineProject.Infrastructure.Persistence;

public static class DatabasePathHelper
{
    private const string DatabaseFileName = "app.db";
    private const string DataFolderName = "data";

    public static string GetDatabasePath()
    {
        var solutionRoot = GetSolutionRoot();
        var dataDirectory = Path.Combine(solutionRoot, DataFolderName);
        return Path.Combine(dataDirectory, DatabaseFileName);
    }

    public static string GetConnectionString()
    {
        var dbPath = GetDatabasePath();
        EnsureDirectoryExists(dbPath);
        return $"Data Source={dbPath}";
    }

    public static void EnsureDirectoryExists(string dbPath)
    {
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static string GetSolutionRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);

        while (directory != null)
        {
            var solutionFile = directory.GetFiles("*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (solutionFile != null)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return currentDirectory;
    }
}
