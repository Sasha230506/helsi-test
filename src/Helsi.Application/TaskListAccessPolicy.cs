using Helsi.Application.Abstractions;
using Helsi.Domain.Entities;

namespace Helsi.Application;

public sealed class TaskListAccessPolicy
{
    private readonly ITaskListRepository _taskListRepo;

    public TaskListAccessPolicy(ITaskListRepository taskListRepo)
    {
        _taskListRepo = taskListRepo;
    }

    public async Task<bool> CanAccessAsync(TaskList list, Guid userId, CancellationToken ct)
    {
        return list.OwnerId == userId || await _taskListRepo.IsUserLinkedAsync(list.Id, userId, ct);
    }

    public Task<bool> CanDeleteAsync(TaskList list, Guid userId, CancellationToken ct)
        => Task.FromResult(list.OwnerId == userId);
}