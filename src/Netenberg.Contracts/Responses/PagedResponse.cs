namespace Netenberg.Contracts.Responses;

public class PagedResponse<T> where T : class
{
    public required IEnumerable<T> Items { get; init; } = [];
    public required int PageSize { get; init; }
    public required int Page { get; init; }
    public required int Total { get; init; }
    public bool HasNextPage => Total > (Page * PageSize);
}
