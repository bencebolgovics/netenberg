namespace Netenberg.Database.Repository;

public interface IReadOnlyRepository<T> where T : class
{
    Task<List<T>> GetAll(CancellationToken cancellationToken);
    Task<List<T>> GetByIds(IEnumerable<int> ids, CancellationToken cancellationToken);
}
