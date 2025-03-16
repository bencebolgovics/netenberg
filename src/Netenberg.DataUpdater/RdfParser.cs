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

#pragma warning disable CS8601 // Possible null reference assignment. (it's checked for null, but visual studio still gives me warnings :(
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

#pragma warning disable CS8604 // Possible null reference argument.
        return new Book()
        {
            GutenbergId = Convert.ToInt32(bookData["ID"]),
            Title = bookData["Title"]?.ToString(),
            Publisher = bookData["Publisher"]?.ToString(),
            PublicationDate = DateTime.Parse(bookData["PublicationDate"]?.ToString()),
            Descriptions = bookData["Descriptions"] as List<string>,
            Rights = bookData["Rights"]?.ToString(),
            Subjects = bookData["Subjects"] as List<string>,
            Language = bookData["Language"]?.ToString(),
            Downloads = Convert.ToInt32(bookData["Downloads"]),
            Urls = bookData["Urls"] as List<string>,
            Bookshelves = bookData["Bookshelves"] as List<string>,
            Authors = ToAuthors(bookData["Creators"] as List<Dictionary<string, object>>),
        };
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    private static Dictionary<string, object> GetCreatorInfo(XElement creator, XNamespace pgterms, XNamespace rdf)
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

        return authors.Select(ToAuthor).ToList();
    }

    private static Author ToAuthor(Dictionary<string, object> author)
    {
        return new Author()
        {
            Name = author["Name"]?.ToString(),
            Alias = author["Alias"]?.ToString(),
            BirthYear = Convert.ToInt32(author["BirthYear"]),
            DeathYear = Convert.ToInt32(author["DeathYear"]),
            Webpage = author["Webpage"]?.ToString()
        };
    }
}