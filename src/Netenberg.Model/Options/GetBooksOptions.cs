using Netenberg.Model.Enums;
namespace Netenberg.Model.Options;

public sealed record class GetBooksOptions
{
    public required string? Ids { get; init; }
    public required string? SortBy { get; init; }
    public SortingOrder SortingOrder => SortBy is null ? SortingOrder.Unsorted : SortBy.Trim().StartsWith('+') ? SortingOrder.Ascending : SortingOrder.Descending;
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
