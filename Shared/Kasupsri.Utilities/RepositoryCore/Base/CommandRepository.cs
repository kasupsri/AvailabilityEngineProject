using Microsoft.EntityFrameworkCore;
using Kasupsri.Utilities.RepositoryCore.Core;

namespace Kasupsri.Utilities.RepositoryCore.Base;

public class CommandRepository<TEntity> : RepositoryBase, ICommandRepository<TEntity> where TEntity : class
{
    public CommandRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Add(entity);

        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task BatchCreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().AddRange(entities);

        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEntitiesForKeysAsync(object[] keys, CancellationToken cancellationToken = default)
    {
        var dbSet = _context.Set<TEntity>();

        var entity = await dbSet.FindAsync(keys, cancellationToken);

        if (entity is null)
            throw new ArgumentNullException($"No Entity Exists for specified Keys: {string.Join(", ", keys)}");

        dbSet.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var dbSet = _context.Set<TEntity>();

        dbSet.Remove(entity);

        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task BatchDeleteEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var dbSet = _context.Set<TEntity>();

        dbSet.RemoveRange(entities);

        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Entry<TEntity>(entity).State = EntityState.Modified;

        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task BatchUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var e in entities)
        {
            _context.Entry<TEntity>(e).State = EntityState.Modified;
        }

        return _context.SaveChangesAsync(cancellationToken);
    }
}
