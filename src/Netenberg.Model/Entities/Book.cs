using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Netenberg.Model.Entities;

public sealed record Book
{
    [BsonId]
    public ObjectId _id { get; set; }
    
    [BsonElement("gutenbergId")]
    public required int GutenbergId { get; set; }

    [BsonElement("title")]
    public required string? Title { get; set; }

    [BsonElement("publisher")]
    public required string? Publisher { get; set; }

    [BsonElement("publicationDate")]
    public required DateTime? PublicationDate { get; set; }

    [BsonElement("downloads")]
    public required int? Downloads { get; set; }

    [BsonElement("rights")]
    public required string? Rights { get; set; }

    [BsonElement("subjects")]
    public required List<string>? Subjects { get; set; }

    [BsonElement("bookshelves")]
    public required List<string>? Bookshelves { get; set; }

    [BsonElement("urls")]
    public required List<string>? Urls { get; set; }

    [BsonElement("language")]
    public required string? Language { get; set; }

    [BsonElement("descriptions")]
    public required List<string>? Descriptions { get; set; }

    [BsonElement("authors")]
    public required List<Author> Authors { get; set; }
}
