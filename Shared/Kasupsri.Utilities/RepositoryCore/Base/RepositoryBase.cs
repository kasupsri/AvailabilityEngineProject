using Microsoft.EntityFrameworkCore;

namespace Kasupsri.Utilities.RepositoryCore.Base;

public class RepositoryBase
{
    protected readonly DbContext _context;

    public RepositoryBase(DbContext dbContext)
    {
        _context = dbContext;
    }

    public void RefreshContext()
    {
        var refreshableObjects = _context.ChangeTracker.Entries().Select(c => c.Entity).ToList();

        foreach (var refreshableObject in refreshableObjects)
        {
            _context.Entry(refreshableObject).Reload();
        }
    }
}
