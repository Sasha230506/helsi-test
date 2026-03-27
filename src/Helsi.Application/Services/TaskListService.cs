using Helsi.Application.Abstractions;
using Helsi.Application.Common;
using Helsi.Domain.Entities;

namespace Helsi.Application.Services;

public class TaskListService : ITaskListService
{
    private readonly ITaskListRepository _taskListRepo;
    private readonly TaskListAccessPolicy _taskListAccessPolicy;

    public TaskListService(
        ITaskListRepository taskListRepo,
        TaskListAccessPolicy taskListAccessPolicy)
    {
        _taskListRepo = taskListRepo;
        _taskListAccessPolicy = taskListAccessPolicy;
    }

    public async Task<Guid> CreateAsync(Guid userId, string title, CancellationToken ct)
    {
        var list = new TaskList(userId, title);

        await _taskListRepo.AddAsync(list, ct);

        return list.Id;
    }

    public async Task<TaskList?> GetByIdAsync(Guid userId, Guid listId, CancellationToken ct)
    {
        var list = await _taskListRepo.GetByIdAsync(listId, ct);
        if (list is null)
            return null;

        if (!await _taskListAccessPolicy.CanAccessAsync(list, userId, ct))
            throw new UnauthorizedAccessException();

        return list;
    }

    public Task<PagedResult<TaskList>> GetPagedAsync(Guid userId, int page, int size, CancellationToken ct)
    {
        if (page < 1) page = 1;
        if (size < 1) size = 20;
        if (size > 100) size = 100;
        return _taskListRepo.GetForUserAsync(userId, page, size, ct);
    }

    public async Task RenameAsync(Guid userId, Guid listId, string title, CancellationToken ct)
    {
        var list = await GetAndAuthorizeAsync(listId, userId, ct);

        list.Rename(title);

        await _taskListRepo.UpdateAsync(list, ct);
    }

    public async Task DeleteAsync(Guid userId, Guid listId, CancellationToken ct)
    {
        var list = await _taskListRepo.GetByIdAsync(listId, ct)
                   ?? throw new KeyNotFoundException();

        if (!await _taskListAccessPolicy.CanDeleteAsync(list, userId, ct))
            throw new UnauthorizedAccessException();

        await _taskListRepo.RemoveAsync(list.Id, ct);
    }

    public async Task AddUserAsync(Guid userId, Guid listId, Guid targetUserId, CancellationToken ct)
    {
        var list = await GetAndAuthorizeAsync(listId, userId, ct);

        if (targetUserId == list.OwnerId)
            throw new InvalidOperationException("Owner already has access");

        if (await _taskListRepo.IsUserLinkedAsync(list.Id, targetUserId, ct))
            throw new InvalidOperationException("User already linked");

        await _taskListRepo.AddUserLinkAsync(list.Id, targetUserId, ct);
    }

    public async Task RemoveUserAsync(Guid userId, Guid listId, Guid targetUserId, CancellationToken ct)
    {
        var list = await GetAndAuthorizeAsync(listId, userId, ct);

        if (targetUserId == list.OwnerId)
            throw new InvalidOperationException("Cannot remove owner");

        await _taskListRepo.RemoveUserLinkAsync(list.Id, targetUserId, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetUsersAsync(Guid userId, Guid listId, CancellationToken ct)
    {
        var list = await GetAndAuthorizeAsync(listId, userId, ct);

        return await _taskListRepo.GetLinkedUsersAsync(list.Id, ct);
    }

    private async Task<TaskList> GetAndAuthorizeAsync(Guid listId, Guid userId, CancellationToken ct)
    {
        var list = await _taskListRepo.GetByIdAsync(listId, ct)
                   ?? throw new KeyNotFoundException();

        if (!await _taskListAccessPolicy.CanAccessAsync(list, userId, ct))
            throw new UnauthorizedAccessException();

        return list;
    }
}