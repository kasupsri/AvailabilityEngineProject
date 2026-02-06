using Kasupsri.Utilities.CQRS.Core;
using Kasupsri.Utilities.RepositoryCore.Core;
using System.Linq.Expressions;

namespace Kasupsri.Utilities.CQRS.Base;

public class FindQuery<TKey, TEntity> : RepositoryQuery<TEntity>, IWhereQuery<TKey, TEntity>
                     where TEntity : class 
{
    public FindQuery(IQueryRepository<TEntity> repository) : base(repository) { }

    public Task<TEntity?> FindAsync(TKey key, CancellationToken cancellationToken = default)
    {
        if (key is null)
            throw new ArgumentNullException(paramName: nameof(key), message: "Key cannot be null");

        return _repository.ReadAsync(key, cancellationToken).AsTask();
    }

    public Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate,
                                    bool refreshContext = false,
                                    CancellationToken cancellationToken = default)
    {        
        return _repository.GetFirstOrDefaultItemsAsync(predicate: predicate,
                                                       refreshContext: refreshContext,
                                                       cancellationToken: cancellationToken);
    }

    public Task<TEntity?> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                    bool refreshContext = false,
                                    CancellationToken cancellationToken = default)
    {
        return _repository.GetFirstOrDefaultItemsAsync(filterQuery: filterQuery,
                                                       refreshContext: refreshContext,
                                                       cancellationToken: cancellationToken);
    }

    public Task<List<TEntity>> GetItemsAsync(Expression<Func<TEntity, bool>> predicate,
                                             bool refreshContext = false,
                                             CancellationToken cancellationToken = default)
    {
        return _repository.GetItemsAsync(predicate: predicate,
                                         refreshContext: refreshContext,
                                         cancellationToken: cancellationToken);
    }

    public Task<List<TEntity>> GetItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                             bool refreshContext = false,
                                             CancellationToken cancellationToken = default)
    {
        return _repository.GetItemsAsync(filterQuery: filterQuery,
                                         refreshContext: refreshContext,
                                         cancellationToken: cancellationToken);
    }

    public Task<int> GetNumberOfItemsAsync(Expression<Func<TEntity, bool>> predicate,
                                           bool refreshContext = false,
                                           CancellationToken cancellationToken = default)
    {
        return _repository.GetNumberOfItemsAsync(predicate: predicate,
                                                 refreshContext: refreshContext,
                                                 cancellationToken: cancellationToken);
    }

    public Task<int> GetNumberOfItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                           bool refreshContext = false,
                                           CancellationToken cancellationToken = default)
    {
        return _repository.GetNumberOfItemsAsync(filterQuery: filterQuery,
                                                 refreshContext: refreshContext,
                                                 cancellationToken: cancellationToken);
    }
}
