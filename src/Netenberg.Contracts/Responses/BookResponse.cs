using Netenberg.Model.Entities;

namespace Netenberg.Contracts.Responses;

public sealed record BookResponse
{
    public required int Id { get; set; }
    public required string? Title { get; set; }
    public required string? Publisher { get; set; }
    public required DateTime? PublicationDate { get; set; }
    public required int? Downloads { get; set; }
    public required string? Rights { get; set; }
    public required List<string>? Subjects { get; set; }
    public required List<string>? Bookshelves { get; set; }
    public required List<string>? Urls { get; set; }
    public required string? Language { get; set; }
    public required List<string>? Descriptions { get; set; }
    public required List<Author> Authors { get; set; }
}
