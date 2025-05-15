using Netenberg.Contracts.Responses;
using Netenberg.Model.Entities;
using Netenberg.Model.Options;

namespace Netenberg.Api.Mapper;

public static class ResponseMapper
{
    public static BookResponse ToBookResponse(this Book book)
    {
        return new BookResponse()
        {
            Id = book.GutenbergId,
            Authors = book.Authors,
            Bookshelves = book.Bookshelves,
            Language = book.Language,
            Publisher = book.Publisher,
            PublicationDate = book.PublicationDate,
            Rights = book.Rights,
            Subjects = book.Subjects,
            Title = book.Title,
            Urls = book.Urls,
            Downloads = book.Downloads,
            Descriptions = book.Descriptions
        };
    }

    public static BooksResponse ToBooksResponse(this IEnumerable<Book> books, GetBooksOptions options, int count)
    {
        return new BooksResponse()
        {
            Items = books.Select(x => x.ToBookResponse()),
            Page = options.Page,
            PageSize = options.PageSize,
            Total = count
        };
    }
}
