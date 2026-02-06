namespace Kasupsri.Utilities.RepositoryCore.Core;

public interface IReadable<TEntity>
{
    ValueTask<TEntity?> ReadAsync(object key, CancellationToken cancellationToken);

    Task<TEntity?> GetFirstOrDefaultItemsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
                                               bool refreshContext = false,
                                               CancellationToken cancellationToken = default);

    Task<TEntity?> GetFirstOrDefaultItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                               bool refreshContext = false,
                                               CancellationToken cancellationToken = default);

    Task<int> GetNumberOfItemsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
                                    bool refreshContext = false,
                                    CancellationToken cancellationToken = default);

    Task<int> GetNumberOfItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                    bool refreshContext = false,
                                    CancellationToken cancellationToken = default);
}
