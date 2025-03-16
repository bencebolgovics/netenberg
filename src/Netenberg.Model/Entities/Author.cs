using MongoDB.Bson.Serialization.Attributes;

namespace Netenberg.Model.Entities;

public sealed record Author
{
    [BsonElement("name")]
    public required string? Name { get; set; }

    [BsonElement("birthYear")]
    public required int? BirthYear { get; set; }

    [BsonElement("deathYear")]
    public required int? DeathYear { get; set; }

    [BsonElement("alias")]
    public required string? Alias { get; set; }

    [BsonElement("webpage")]
    public required string? Webpage { get; set; }
}
