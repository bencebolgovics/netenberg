using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Netenberg.Model.Entities;

public sealed record Book
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    [BsonElement("gutenbergId")]
    public required int GutenbergId { get; set; }

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("description")]
    public required string Description { get; set; }

    [BsonElement("authors")]
    public required List<Author> Authors { get; set; }
}
