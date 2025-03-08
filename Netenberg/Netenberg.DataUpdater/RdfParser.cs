using Netenberg.Model.Models;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Netenberg.DataUpdater;

public static class RdfParser
{
    public static Book ParseBookFromRdf(string filePath)
    {
        Graph graph = new();
        FileLoader.Load(graph, filePath);

        var rdfType = graph.CreateUriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
        var ebookType = graph.CreateUriNode(new Uri("http://www.gutenberg.org/2009/pgterms/ebook"));
        var titlePredicate = graph.CreateUriNode(new Uri("http://purl.org/dc/terms/title"));
        var descriptionPredicate = graph.CreateUriNode(new Uri("http://purl.org/dc/terms/description"));
        var creatorPredicate = graph.CreateUriNode(new Uri("http://purl.org/dc/terms/creator"));
        var namePredicate = graph.CreateUriNode(new Uri("http://www.gutenberg.org/2009/pgterms/name"));

        var ebookTriple = graph.GetTriplesWithPredicateObject(rdfType, ebookType).FirstOrDefault();

        ArgumentNullException.ThrowIfNull(ebookTriple);

        var ebookNode = ebookTriple.Subject;

        int gutenbergId = -1;

        if (ebookNode is UriNode uriNode)
        {
            string uriStr = uriNode.Uri.ToString();
            int idx = uriStr.IndexOf("ebooks/");
            if (idx >= 0)
            {
                gutenbergId = Convert.ToInt32(uriStr[(idx + "ebooks/".Length)..]);
            }
            else
            {
                gutenbergId = Convert.ToInt32(uriStr);
            }
        }
        else
        {
            gutenbergId = Convert.ToInt32(ebookNode.ToString());
        }

        var titleTriple = graph.GetTriplesWithSubjectPredicate(ebookNode, titlePredicate).FirstOrDefault();
        string title = "asd";

        var descriptionTriples = graph.GetTriplesWithSubjectPredicate(ebookNode, descriptionPredicate).ToList();
        string description = string.Join("\n", descriptionTriples
            .Select(triple => triple.Object is LiteralNode lit ? lit.Value : string.Empty)
            .Where(s => !string.IsNullOrEmpty(s)));

        string authorName = string.Empty;
        var creatorTriple = graph.GetTriplesWithSubjectPredicate(ebookNode, creatorPredicate).FirstOrDefault();
        if (creatorTriple != null)
        {
            var agentNode = creatorTriple.Object;
            var nameTriple = graph.GetTriplesWithSubjectPredicate(agentNode, namePredicate).FirstOrDefault();
            if (nameTriple != null && nameTriple.Object is LiteralNode nameLiteral)
            {
                authorName = nameLiteral.Value;
            }
        }

        return new Book
        {
            GutenbergId = gutenbergId,
            Title = title,
            Description = description,
            Author = authorName
        };
    }
}
