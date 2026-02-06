namespace Kasupsri.Utilities.RepositoryCore.Core;

public interface ICreatable<TEntity>
{
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task BatchCreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
