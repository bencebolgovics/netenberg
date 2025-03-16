using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netenberg.Database.Repositories;
using Netenberg.Model.Entities;
using System.Collections.Concurrent;

namespace Netenberg.DataUpdater;

public class DataUpdaterService
{
    private readonly IBookRepository _bookRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataUpdaterService> _logger;

    private static readonly ConcurrentQueue<Book> buffer = new();
    private static readonly SemaphoreSlim bufferLock = new(1, 1);
    
    public DataUpdaterService(IBookRepository bookRepository,
                              IServiceScopeFactory scopeFactory,
                              ILogger<DataUpdaterService> logger)
    {
        _bookRepository = bookRepository;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    public async Task UpdateDatabase(CancellationToken cancellationToken)
    {
        await Parallel.ForAsync(1, 80000, new ParallelOptions() { MaxDegreeOfParallelism = 7 }, async (index, cancellationToken) =>
        {
            using var scope = _scopeFactory.CreateScope();
            cancellationToken.ThrowIfCancellationRequested();

            string filePath = $"C:/books/books/cache/epub/{index}/pg{index}.rdf";

            if (!File.Exists(filePath))
                return;

            var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();

            var isBookExists = await bookRepository.Exists(index, cancellationToken);
            if (isBookExists == true)
                return;

            var book = RdfParser.ParseRdfToBook(filePath);
            if (book != null)
            {
                buffer.Enqueue(book);
                _logger.LogInformation($"Enqueueing book {index}");

                if (buffer.Count >= 500)
                {
                    await bufferLock.WaitAsync(cancellationToken);
                    try
                    {
                        await bookRepository.CreateMany(buffer, cancellationToken);
                        _logger.LogInformation($"Creating buffer");
                        buffer.Clear();
                    }
                    finally
                    {
                        bufferLock.Release();
                    }
                }
            }
        });

        if (!buffer.IsEmpty)
        {
            await _bookRepository.CreateMany(buffer, cancellationToken);
            buffer.Clear();
        }
    }
}
