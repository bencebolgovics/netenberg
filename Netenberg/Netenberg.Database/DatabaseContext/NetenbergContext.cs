using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Netenberg.Model.Models;

namespace Netenberg.Database.DatabaseContext;

public sealed class NetenbergContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Book> Books { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Book>().ToCollection("books");
    }
}
