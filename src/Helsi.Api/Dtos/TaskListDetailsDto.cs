namespace Helsi.Api.Dtos;

public sealed record TaskListDetailsDto(Guid Id, string Title, Guid OwnerId, DateTime CreatedAtUtc);