using Netenberg.Database.Repositories;
using Netenberg.Model.Entities;
using Netenberg.Model.Options;

namespace Netenberg.Application.Services;

public sealed class BooksService : IBooksService
{
    private readonly IReadOnlyBookRepository _booksReadOnlyRepository;

    public BooksService(IReadOnlyBookRepository booksReadOnlyRepository)
    {
        _booksReadOnlyRepository = booksReadOnlyRepository;
    }

    public async Task<IEnumerable<Book>> GetBooks(GetBooksOptions options, CancellationToken cancellationToken)
    {
        return await _booksReadOnlyRepository.GetAll(options, cancellationToken);
    }

    public async Task<Book?> GetBook(int id, CancellationToken cancellationToken)
    {
        return await _booksReadOnlyRepository.GetById(id, cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken)
    {
        return await _booksReadOnlyRepository.GetCountAsync(cancellationToken);
    }
}
