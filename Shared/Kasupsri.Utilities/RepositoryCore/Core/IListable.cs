namespace Kasupsri.Utilities.RepositoryCore.Core;

public interface IListable<TEntity>
{
    IQueryable<TEntity> GetQueryable(bool refreshContext = false);

    Task<List<TEntity>> GetItemsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
                                      bool refreshContext = false,
                                      CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                      bool refreshContext = false,
                                      CancellationToken cancellationToken = default);
}
