using Netenberg.Model.Enums;
namespace Netenberg.Model.Models;

public sealed record class GetBooksOptions
{
    public required string? Ids { get; init; }
    public required string? SortBy { get; init; }
    public required SortingOrder SortingOrder { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
