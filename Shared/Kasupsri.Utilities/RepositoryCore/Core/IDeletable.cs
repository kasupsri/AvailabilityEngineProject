namespace Kasupsri.Utilities.RepositoryCore.Core;

public interface IDeletable<TEntity>
{
    Task DeleteEntitiesForKeysAsync(object[] keys, CancellationToken cancellationToken = default);

    Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task BatchDeleteEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
