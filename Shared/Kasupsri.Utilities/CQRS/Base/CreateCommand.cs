using Kasupsri.Utilities.CQRS.Core;
using Kasupsri.Utilities.RepositoryCore.Core;

namespace Kasupsri.Utilities.CQRS.Base;

public class CreateCommand<TEntity> : RepositoryCommand<TEntity>, ICreateCommand<TEntity>
     where TEntity : class
{
    public CreateCommand(ICommandRepository<TEntity> repository) : base(repository) { }

    public Task CreateEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _repository.CreateAsync(entity, cancellationToken);
    }

    public Task CreateEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _repository.BatchCreateAsync(entities, cancellationToken);
    }
}
