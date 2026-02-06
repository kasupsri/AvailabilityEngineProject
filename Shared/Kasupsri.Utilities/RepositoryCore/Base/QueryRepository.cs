using Microsoft.EntityFrameworkCore;
using Kasupsri.Utilities.RepositoryCore.Core;
using System.Linq.Expressions;

namespace Kasupsri.Utilities.RepositoryCore.Base;

public class QueryRepository<TEntity> : RepositoryBase, IQueryRepository<TEntity>
    where TEntity : class
{
    public QueryRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public ValueTask<TEntity?> ReadAsync(object key,
                                        CancellationToken cancellationToken = default)
    {
        return _context.Set<TEntity>().FindAsync(new object[] { key }, cancellationToken: cancellationToken);
    }

    public IQueryable<TEntity> GetQueryable(bool refreshContext = false)
    {
        if (refreshContext)
        {
            base.RefreshContext();
        }
        var query = _context.Set<TEntity>().AsQueryable();
        
        if (!refreshContext)
        {
            query = query.AsNoTracking();
        }
        
        return query;
    }

    public Task<List<TEntity>> GetItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                             bool refreshContext = false,
                                             CancellationToken cancellationToken = default)
    {
        return filterQuery(GetQueryable(refreshContext))
               .ToListAsync(cancellationToken);
    }

    public Task<TEntity?> GetFirstOrDefaultItemsAsync(Expression<Func<TEntity, bool>> predicate,
                                                      bool refreshContext = false,
                                                      CancellationToken cancellationToken = default)
    {
        return GetQueryable(refreshContext)
                .Where(predicate)
                .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<TEntity>> GetItemsAsync(Expression<Func<TEntity, bool>> predicate,
                                             bool refreshContext = false,
                                             CancellationToken cancellationToken = default)
    {
        return GetQueryable(refreshContext)
                .Where(predicate)
                .ToListAsync(cancellationToken);
    }

    public Task<TEntity?> GetFirstOrDefaultItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery,
                                                      bool refreshContext = false,
                                                      CancellationToken cancellationToken = default)
    {
        return filterQuery(GetQueryable(refreshContext))
               .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> GetNumberOfItemsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filterQuery, 
                                           bool refreshContext = false, 
                                           CancellationToken cancellationToken = default)
    {
        return filterQuery(GetQueryable(refreshContext))
                    .CountAsync(cancellationToken);
    }

    public Task<int> GetNumberOfItemsAsync(Expression<Func<TEntity, bool>> predicate,
                                           bool refreshContext = false,
                                           CancellationToken cancellationToken = default)
    {
        return GetQueryable(refreshContext)
                    .Where(predicate)
                    .CountAsync(cancellationToken);
    }
}
