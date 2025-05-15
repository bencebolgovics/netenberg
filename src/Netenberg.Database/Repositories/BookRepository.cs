using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Netenberg.Database.DatabaseContext;
using Netenberg.Database.Extensions;
using Netenberg.Model.Entities;
using Netenberg.Model.Models;

namespace Netenberg.Database.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly NetenbergContext _dbContext;

    public BookRepository(NetenbergContext dbContext)
    {
        _dbContext = dbContext;
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
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entities;
    }

    public async Task Delete(Book entity, CancellationToken cancellationToken)
    {
        _dbContext.Books.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Book>> GetAll(GetBooksOptions options, CancellationToken cancellationToken)
    {
        IQueryable<Book> query = _dbContext.Books.AsNoTracking();

        if (!string.IsNullOrEmpty(options.Ids))
        {
            var ids = options.Ids.Split(',').Select(id => Convert.ToInt32(id));
            query = query.Where(x => ids.Contains(x.GutenbergId));
        }

        if (!string.IsNullOrEmpty(options.SortBy))
        {
            query = query.OrderByField(options.SortBy, options.SortingOrder);
        }

        query = query.Skip((options.Page - 1) * options.PageSize).Take(options.PageSize);

        return await query.ToListAsync(cancellationToken);
    }
    
    public async Task<Book?> GetById(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Books.AsNoTracking()
            .FirstOrDefaultAsync(b => b.GutenbergId == id, cancellationToken);
    }

    public async Task<bool> Exists(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Books.AsNoTracking()
            .AnyAsync(b => b.GutenbergId == id, cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Books.AsNoTracking()
            .CountAsync(cancellationToken);
    }

    public async Task<Book> Update(Book entity, CancellationToken cancellationToken)
    {
        _dbContext.Books.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
