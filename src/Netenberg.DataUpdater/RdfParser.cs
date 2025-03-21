using Netenberg.Model.Entities;
using System.Xml.Linq;

namespace Netenberg.DataUpdater;

public class RdfParser
{
    public static Book ParseRdfToBook(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);

        XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        XNamespace dcterms = "http://purl.org/dc/terms/";
        XNamespace pgterms = "http://www.gutenberg.org/2009/pgterms/";
        XNamespace dcam = "http://purl.org/dc/dcam/";
        XNamespace cc = "http://web.resource.org/cc/";

        var ebook = doc.Descendants(pgterms + "ebook").FirstOrDefault();
        ArgumentNullException.ThrowIfNull(ebook);

        var bookData = new Dictionary<string, object>
        {
            ["ID"] = ebook.Attribute(rdf + "about")?.Value.Split('/').Last(),
            ["Title"] = ebook.Element(dcterms + "title")?.Value,
            ["Publisher"] = ebook.Element(dcterms + "publisher")?.Value,
            ["PublicationDate"] = ebook.Element(dcterms + "issued")?.Value,
            ["Downloads"] = ebook.Element(pgterms + "downloads")?.Value,
            ["Rights"] = ebook.Element(dcterms + "rights")?.Value,
            ["Language"] = ebook.Element(dcterms + "language")?
                .Element(rdf + "Description")?
                .Element(rdf + "value")?.Value,
            ["Descriptions"] = ebook.Elements(dcterms + "description")
                .Select(d => d.Value.Trim())
                .ToList(),
            ["Subjects"] = ebook.Elements(dcterms + "subject")
                .Select(s => s.Element(rdf + "Description")?.Element(rdf + "value")?.Value)
                .Where(value => value != null)
                .ToList(),
            ["Urls"] = ebook.Elements(dcterms + "hasFormat")
                .Select(f => f.Element(pgterms + "file")?.Attribute(rdf + "about")?.Value)
                .Where(url => url != null)
                .ToList(),
            ["Bookshelves"] = ebook.Elements(pgterms + "bookshelf")
                .Select(b => b.Element(rdf + "Description")?
                    .Element(rdf + "value")?.Value)
                .ToList(),
            ["Creators"] = ebook.Elements(dcterms + "creator")
                .Select(c => GetCreatorInfo(c.Element(pgterms + "agent"), pgterms, rdf))
                .Where(creator => creator != null) // Filter out null values
                .ToList(),
        };

        return new Book()
        {
            GutenbergId = Convert.ToInt32(bookData["ID"]),
            Title = bookData["Title"]?.ToString(),
            Publisher = bookData["Publisher"]?.ToString(),
            PublicationDate = bookData["PublicationDate"] is not null ? DateTime.Parse(bookData["PublicationDate"].ToString()!) : null,
            Descriptions = bookData["Descriptions"] as List<string> ?? [],
            Rights = bookData["Rights"]?.ToString() ?? string.Empty,
            Subjects = bookData["Subjects"] as List<string> ?? [],
            Language = bookData["Language"]?.ToString() ?? string.Empty,
            Downloads = Convert.ToInt32(bookData["Downloads"]),
            Urls = bookData["Urls"] as List<string> ?? [],
            Bookshelves = bookData["Bookshelves"] as List<string> ?? [],
            Authors = ToAuthors(bookData["Creators"] as List<Dictionary<string, object>> ?? []),
        };
    }

    private static Dictionary<string, object>? GetCreatorInfo(XElement creator, XNamespace pgterms, XNamespace rdf)
    {
        if (creator == null)
            return null;

        return new Dictionary<string, object>
        {
            ["Name"] = creator.Element(pgterms + "name")?.Value,
            ["BirthYear"] = creator.Element(pgterms + "birthdate")?.Value,
            ["DeathYear"] = creator.Element(pgterms + "deathdate")?.Value,
            ["Alias"] = creator.Element(pgterms + "alias")?.Value,
            ["Webpage"] = creator.Element(pgterms + "webpage")?.Attribute(rdf + "resource")?.Value
        };
    }

    private static List<Author> ToAuthors(List<Dictionary<string, object>> authors)
    {
        if (authors is null)
            return [];

        return [.. authors.Select(ToAuthor)];
    }

    private static Author ToAuthor(Dictionary<string, object> author)
    {
        return new Author()
        {
            Name = author["Name"]?.ToString(),
            Alias = author["Alias"]?.ToString(),
            BirthYear = author["BirthYear"] is not null ? Convert.ToInt32(author["BirthYear"]) : null,
            DeathYear = author["DeathYear"] is not null ? Convert.ToInt32(author["DeathYear"]) : null,
            Webpage = author["Webpage"]?.ToString()
        };
    }
}