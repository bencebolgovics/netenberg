using Netenberg.Model.Entities;
using Netenberg.Model.Options;

namespace Netenberg.Database.Repositories;

public interface IReadOnlyBookRepository
{
    Task<List<Book>> GetAll(GetBooksOptions options, CancellationToken cancellationToken);
    Task<Book?> GetById(int id, CancellationToken cancellationToken);
    Task<int> GetCountAsync(CancellationToken cancellationToken);
    Task<bool> Exists(int id, CancellationToken cancellationToken);
}

