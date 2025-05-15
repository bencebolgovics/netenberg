using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netenberg.Database.Repositories;
using Netenberg.Model.Entities;
using Quartz;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Netenberg.Application.Jobs;

public class DataUpdaterJob : IJob
{
    private readonly IBookRepository _bookRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataUpdaterJob> _logger;

    private static readonly ConcurrentQueue<Book> _buffer = new();
    private static readonly SemaphoreSlim _bufferLock = new(1, 1);

    private static readonly string _path = "https://gutenberg.org/cache/epub/feeds/rdf-files.tar.zip";
    private static readonly int _bufferSize = 500;

    public DataUpdaterJob(IBookRepository bookRepository,
                              IServiceScopeFactory scopeFactory,
                              ILogger<DataUpdaterJob> logger)
    {
        _bookRepository = bookRepository;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await DownloadBooks();

        UnzipBooks();

        await UploadBooks(context.CancellationToken);

        File.Delete("books.zip");
        Directory.Delete("books", true);
        _logger.LogInformation("Books updated successfully.");
    }

    private async Task UploadBooks(CancellationToken cancellationToken)
    {
        var count = _bookRepository.GetCountAsync(cancellationToken);

        var directories = Directory.EnumerateDirectories("books/cache/epub", "*", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(directories, new ParallelOptions() { MaxDegreeOfParallelism = 7 }, async (directory, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _scopeFactory.CreateScope();
            var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();

            var filePath = Directory.GetFiles(directory)
                .Single(x => x.EndsWith("rdf"));

            var bookId = GetIdFromPath(filePath);

            var isBookExists = await bookRepository.Exists(bookId, cancellationToken);
            if (isBookExists == true)
                return;

            var book = RdfParser.ParseRdfToBook(filePath);
            if (book != null)
            {
                _buffer.Enqueue(book);
                _logger.LogInformation($"Enqueueing book {bookId}");

                if (_buffer.Count >= _bufferSize)
                {
                    await _bufferLock.WaitAsync(cancellationToken);
                    try
                    {
                        await bookRepository.CreateMany(_buffer, cancellationToken);
                        _logger.LogInformation($"Creating buffer");
                        _buffer.Clear();
                    }
                    finally
                    {
                        _bufferLock.Release();
                    }
                }
            }
        });

        if (!_buffer.IsEmpty)
        {
            await _bookRepository.CreateMany(_buffer, cancellationToken);
            _buffer.Clear();
        }
    }

    private async Task DownloadBooks()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(_path);

        if (response.IsSuccessStatusCode)
        {
            using var fileStream = new FileStream("books.zip", FileMode.Create, FileAccess.Write);
            response.Content.CopyToAsync(fileStream).Wait();
            _logger.LogInformation("Books downloaded successfully.");
        }
        else
        {
            _logger.LogError($"Failed to download books: {response.StatusCode}");
        }
    }

    private static int GetIdFromPath(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        Match match = Regex.Match(fileName, @"\d+");

        if (match.Success)
        {
            return int.Parse(match.Value);
        }

        throw new InvalidOperationException($"Could not extract ID from file name: {fileName}");
    }

    private void UnzipBooks()
    {
        Directory.CreateDirectory("books");

        ZipFile.ExtractToDirectory("books.zip", "books");

        using var archive = ArchiveFactory.Open("books/rdf-files.tar");
        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory)
            {
                entry.WriteToDirectory("books", new ExtractionOptions
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }

            _logger.LogInformation($"Extracting {entry.Key}");
        }

        Directory.Delete("books/cache/epub/test", true);
    }
}
