namespace Kasupsri.Utilities.CQRS.Core;

public interface IFindQuery<TKey, TEntity>
{
    Task<TEntity?> FindAsync(TKey key,
                             CancellationToken cancellationToken = default);
}
