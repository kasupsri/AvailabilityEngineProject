namespace Kasupsri.Utilities.CQRS.Core;

public interface IUpdateCommand<TEntity>
        where TEntity : class
{
    Task UpdateEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
}
