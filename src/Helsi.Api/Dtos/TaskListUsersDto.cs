namespace Helsi.Api.Dtos;

public sealed record TaskListUsersDto(IReadOnlyList<Guid> UserIds);