namespace cookbook.Models;

public abstract record PagedResponsesDTO<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = [];
    public int PageSize { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
    public int TotalPages { get; init; }
}
