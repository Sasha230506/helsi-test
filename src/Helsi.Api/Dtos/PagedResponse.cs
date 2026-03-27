namespace Helsi.Api.Dtos;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int Size);