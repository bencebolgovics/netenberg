using Netenberg.Model.Models;

namespace Netenberg.Application.Services;

public interface IBooksService
{
    public Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken);
}
