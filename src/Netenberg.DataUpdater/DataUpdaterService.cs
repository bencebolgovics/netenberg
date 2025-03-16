using Microsoft.Extensions.Logging;
using Netenberg.Database.Repositories;
using Netenberg.Model.Entities;
using System.Collections.Concurrent;

namespace Netenberg.DataUpdater;

public class DataUpdaterService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<DataUpdaterService> _logger;

    private static readonly ConcurrentQueue<Book> buffer = new();
    private static readonly SemaphoreSlim bufferLock = new(1, 1);

    public DataUpdaterService(IBookRepository bookRepository, ILogger<DataUpdaterService> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task UpdateDatabase(CancellationToken cancellationToken)
    {
        await Parallel.ForAsync(1, 1000, async (index, cancellationToken) =>
        {
            var bookRepository = new BookRepository();

            cancellationToken.ThrowIfCancellationRequested();

            var bookFromDb = await bookRepository.GetById(index, cancellationToken);
            if (bookFromDb != null)
                return;

            var book = RdfParser.ParseRdfToBook($"C:/books/books/cache/epub/{index}/pg{index}.rdf");
            if (book != null)
            {
                buffer.Enqueue(book);
                _logger.LogInformation($"Enqueueing book {index}");

                if (buffer.Count >= 100)
                {
                    await bufferLock.WaitAsync(cancellationToken);
                    try
                    {
                        if (buffer.Count >= 100)
                        {
                            var booksToInsert = buffer.ToList();
                            buffer.Clear();
                            await bookRepository.CreateMany(booksToInsert, cancellationToken);
                            _logger.LogInformation($"Creating buffer");
                        }
                    }
                    finally
                    {
                        bufferLock.Release();
                    }
                }
            }
        });

        if (buffer.IsEmpty)
        {
            var booksToInsert = buffer.ToList();
            buffer.Clear();
            await _bookRepository.CreateMany(booksToInsert, CancellationToken.None);
        }
    }
}
