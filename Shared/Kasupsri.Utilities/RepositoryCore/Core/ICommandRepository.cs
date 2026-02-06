namespace Kasupsri.Utilities.RepositoryCore.Core;

public interface ICommandRepository<TEntity> : ICreatable<TEntity>, IUpdatable<TEntity>, IDeletable<TEntity>
{
}
