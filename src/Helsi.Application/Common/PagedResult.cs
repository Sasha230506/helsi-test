namespace Helsi.Application.Common;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Total { get; }
    public int Page { get; }
    public int Size { get; }

    public PagedResult(IReadOnlyList<T> items, int total, int page, int size)
    {
        Items = items;
        Total = total;
        Page = page;
        Size = size;
    }
}