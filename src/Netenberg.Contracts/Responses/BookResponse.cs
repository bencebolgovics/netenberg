using Netenberg.Model.Entities;

namespace Netenberg.Contracts.Responses;

public sealed record BookResponse
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required List<Author> Authors { get; init; }
}
