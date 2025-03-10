using Netenberg.Model.Entities;

namespace Netenberg.Database.Repositories;

public interface IBookRepository : IReadOnlyBookRepository
{
    Task<Book> Create(Book entity, CancellationToken cancellationToken);
    Task<IEnumerable<Book>> CreateMany(IEnumerable<Book> entities, CancellationToken cancellationToken);
    Task Delete(Book entity, CancellationToken cancellationToken);
    Task<Book> Update(Book entity, CancellationToken cancellationToken);
}
