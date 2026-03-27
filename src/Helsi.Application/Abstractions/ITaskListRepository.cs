using Helsi.Application.Common;
using Helsi.Domain.Entities;

namespace Helsi.Application.Abstractions;

public interface ITaskListRepository
{
    Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<TaskList>> GetForUserAsync(Guid userId, int page, int size, CancellationToken ct);

    Task AddAsync(TaskList entity, CancellationToken ct);
    Task UpdateAsync(TaskList entity, CancellationToken ct);
    Task RemoveAsync(Guid id, CancellationToken ct);

    Task<bool> IsUserLinkedAsync(Guid listId, Guid userId, CancellationToken ct);
    Task AddUserLinkAsync(Guid listId, Guid userId, CancellationToken ct);
    Task RemoveUserLinkAsync(Guid listId, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetLinkedUsersAsync(Guid listId, CancellationToken ct);
}