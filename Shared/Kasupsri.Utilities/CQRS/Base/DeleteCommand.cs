using Kasupsri.Utilities.CQRS.Core;
using Kasupsri.Utilities.RepositoryCore.Core;

namespace Kasupsri.Utilities.CQRS.Base;

public class DeleteCommand<TEntity> : RepositoryCommand<TEntity>, IDeleteCommand<TEntity>
             where TEntity : class
{
    public DeleteCommand(ICommandRepository<TEntity> repository) : base(repository) { }

    public Task DeleteEntitiesForKeysAsync(object[] keys, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteEntitiesForKeysAsync(keys, cancellationToken);
    }

    public Task DeleteEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _repository.BatchDeleteEntitiesAsync(entities, cancellationToken);
    }

    public Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteEntityAsync(entity, cancellationToken);
    }
}
