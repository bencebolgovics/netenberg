using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Netenberg.Model.Models;

public sealed class Book
{
    [BsonId]
    public required ObjectId _id { get; set; }

    [BsonElement("gutenbergId")]
    public required int GutenbergId { get; set; }

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("description")]
    public required string Description { get; set; }

    [BsonElement("author")]
    public required string Author { get; set; }
}
