using Kasupsri.Utilities.CQRS.Core;
using Kasupsri.Utilities.RepositoryCore.Core;

namespace Kasupsri.Utilities.CQRS.Base;

public class UpdateCommand<TEntity> : IUpdateCommand<TEntity>
    where TEntity : class
{
    public UpdateCommand(ICommandRepository<TEntity> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    private readonly ICommandRepository<TEntity> _repository;

    public Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(entity, cancellationToken);
    }

    public Task UpdateEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _repository.BatchUpdateAsync(entities, cancellationToken);
    }
}
