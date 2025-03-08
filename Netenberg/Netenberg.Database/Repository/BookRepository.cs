using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Netenberg.Database.DatabaseContext;
using Netenberg.Model.Models;

namespace Netenberg.Database.Repository;

public sealed class BookRepository : IRepository<Book>
{
    private readonly NetenbergContext _dbContext;
    string connectionString = "mongodb+srv://bencebolgovics:qfnWAeMbUhibP5Q@netenberg.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000";
    string databaseName = "netenbergDB";

    public BookRepository()
    {
        var client = new MongoClient(connectionString);
        var dbContextOptions = new DbContextOptionsBuilder<NetenbergContext>()
            .UseMongoDB(client, databaseName).Options;
        _dbContext = new NetenbergContext(dbContextOptions);
    }

    public async Task<Book> Create(Book entity, CancellationToken cancellationToken)
    {
        _dbContext.Books.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<IEnumerable<Book>> CreateMany(IEnumerable<Book> entities, CancellationToken cancellationToken)
    {
        await _dbContext.Books.AddRangeAsync(entities, cancellationToken);

        return entities;
    }

    public async Task Delete(Book entity, CancellationToken cancellationToken)
    {
        _dbContext.Books.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Book>> GetAll(CancellationToken cancellationToken)
    {
        return await _dbContext.Books.ToListAsync(cancellationToken);
    }

    public Task<List<Book>> GetByIds(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Book> Update(Book entity, CancellationToken cancellationToken)
    {
        _dbContext.Books.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
