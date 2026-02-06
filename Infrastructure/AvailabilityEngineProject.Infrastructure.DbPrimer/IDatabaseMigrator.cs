namespace AvailabilityEngineProject.Infrastructure.DbPrimer;

public interface IDatabaseMigrator
{
    bool Upgrade();
}
