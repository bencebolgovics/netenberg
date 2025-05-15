using Netenberg.Model.Entities;
using Netenberg.Model.Options;

namespace Netenberg.Application.Services;

public interface IBooksService
{
    public Task<IEnumerable<Book>> GetBooks(GetBooksOptions options, CancellationToken cancellationToken);
    public Task<Book?> GetBook(int id, CancellationToken cancellationToken);
    public Task<int> GetCountAsync(CancellationToken cancellationToken);
}
