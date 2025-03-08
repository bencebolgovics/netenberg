using Netenberg.Database.Repository;
using Netenberg.Model.Models;

namespace Netenberg.Application.Services;

public sealed class BooksService : IBooksService
{
    private readonly IReadOnlyRepository<Book> _booksReadOnlyRepository;

    public BooksService(IReadOnlyRepository<Book> booksReadOnlyRepository)
    {
        _booksReadOnlyRepository = booksReadOnlyRepository;   
    }

    public async Task<IEnumerable<Book>> GetBooks(CancellationToken cancellationToken)
    {
        return await _booksReadOnlyRepository.GetAll(cancellationToken);
    }
}
