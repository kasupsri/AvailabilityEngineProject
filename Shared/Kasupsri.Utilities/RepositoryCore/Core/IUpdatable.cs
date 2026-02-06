namespace Kasupsri.Utilities.RepositoryCore.Core;

public interface IUpdatable<TEntity>
{
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task BatchUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
