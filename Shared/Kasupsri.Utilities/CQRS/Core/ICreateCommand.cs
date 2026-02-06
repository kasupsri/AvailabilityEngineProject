namespace Kasupsri.Utilities.CQRS.Core;

public interface ICreateCommand<TEntity>
        where TEntity : class
{
    Task CreateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task CreateEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
