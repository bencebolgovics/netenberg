namespace Netenberg.Database.Repository;

public interface IRepository<T> : IReadOnlyRepository<T> where T : class
{
    Task<T> Create(T entity, CancellationToken cancellationToken);
    Task<IEnumerable<T>> CreateMany(IEnumerable<T> entities, CancellationToken cancellationToken);
    Task Delete(T entity, CancellationToken cancellationToken);
    Task<T> Update(T entity, CancellationToken cancellationToken);
}
