namespace Kasupsri.Utilities.CQRS.Core;

public interface IWhereQuery<TKey, TEntity> : IFindQuery<TKey, TEntity>
{
    Task<TEntity?> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
                             bool refreshContext = false,
                             CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                             bool refreshContext = false,
                             CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetItemsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
                                      bool refreshContext = false,
                                      CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                      bool refreshContext = false,
                                      CancellationToken cancellationToken = default);

    Task<int> GetNumberOfItemsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
                                    bool refreshContext = false,
                                    CancellationToken cancellationToken = default);

    Task<int> GetNumberOfItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                    bool refreshContext = false,
                                    CancellationToken cancellationToken = default);
}
