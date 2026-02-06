using Kasupsri.Utilities.RepositoryCore.Core;

namespace Kasupsri.Utilities.CQRS.Base;

public abstract class RepositoryQuery<TEntity>
         where TEntity : class
{
    protected RepositoryQuery(IQueryRepository<TEntity> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    protected readonly IQueryRepository<TEntity> _repository;
}
