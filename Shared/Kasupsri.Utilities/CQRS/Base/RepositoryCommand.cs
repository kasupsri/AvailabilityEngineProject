using Kasupsri.Utilities.RepositoryCore.Core;

namespace Kasupsri.Utilities.CQRS.Base;

public abstract class RepositoryCommand<TEntity>
             where TEntity : class
{
    protected RepositoryCommand(ICommandRepository<TEntity> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    protected readonly ICommandRepository<TEntity> _repository;
}
