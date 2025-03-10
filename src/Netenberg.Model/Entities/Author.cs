using MongoDB.Bson.Serialization.Attributes;

namespace Netenberg.Model.Entities;

public sealed record Author
{
    [BsonElement("name")]
    public required string Name { get; set; }
}
