using Helsi.Application.Common;
using Helsi.Domain.Entities;

namespace Helsi.Application.Abstractions;

public interface ITaskListService
{
    Task<Guid> CreateAsync(Guid userId, string title, CancellationToken ct);
    Task<TaskList?> GetByIdAsync(Guid userId, Guid listId, CancellationToken ct);
    Task<PagedResult<TaskList>> GetPagedAsync(Guid userId, int page, int size, CancellationToken ct);
    Task RenameAsync(Guid userId, Guid listId, string title, CancellationToken ct);
    Task DeleteAsync(Guid userId, Guid listId, CancellationToken ct);
    Task AddUserAsync(Guid userId, Guid listId, Guid targetUserId, CancellationToken ct);
    Task RemoveUserAsync(Guid userId, Guid listId, Guid targetUserId, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetUsersAsync(Guid userId, Guid listId, CancellationToken ct);
}