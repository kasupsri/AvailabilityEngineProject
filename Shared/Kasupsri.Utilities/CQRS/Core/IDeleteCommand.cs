namespace Kasupsri.Utilities.CQRS.Core;

public interface IDeleteCommand<TEntity>
            where TEntity : class
{
    Task DeleteEntitiesForKeysAsync(object[] keys, CancellationToken cancellationToken = default);

    Task DeleteEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
}
