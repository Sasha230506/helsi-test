using Helsi.Api.Dtos;
using Helsi.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Helsi.Api.Controllers;

[ApiController]
[Route("tasklists")]
public class TaskListsController : ControllerBase
{
    private readonly ITaskListService _taskListService;

    public TaskListsController(ITaskListService taskListService)
    {
        _taskListService = taskListService;
    }

    /// <summary>
    /// Create a task list for the current user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader]Guid userId,
        [FromBody] CreateTaskListRequest request,
        CancellationToken ct)
    {
        var id = await _taskListService.CreateAsync(userId, request.Title, ct);
        return CreatedAtAction(nameof(GetById), new { taskListId = id }, new { id });
    }

    /// <summary>
    /// Returns user task lists with paging support.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromHeader]Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var paged = await _taskListService.GetPagedAsync(userId, page, size, ct);

        var items = paged.Items
            .Select(x => new TaskListItemDto(x.Id, x.Title))
            .ToList();

        return Ok(new PagedResponse<TaskListItemDto>(items, paged.Total, paged.Page, paged.Size));
    }

    /// <summary>
    /// Returns information about a specific task list.
    /// </summary>
    [HttpGet("{taskListId:guid}")]
    public async Task<IActionResult> GetById([FromHeader]Guid userId, Guid taskListId, CancellationToken ct)
    {
        var list = await _taskListService.GetByIdAsync(userId, taskListId, ct);
        if (list is null)
            return NotFound();

        return Ok(new TaskListDetailsDto(list.Id, list.Title, list.OwnerId, list.CreatedAtUtc));
    }

    /// <summary>
    /// Updates the title of the task list.
    /// </summary>
    [HttpPut("{taskListId:guid}")]
    public async Task<IActionResult> Rename(
        [FromHeader]Guid userId,
        Guid taskListId,
        [FromBody] RenameTaskListRequest request,
        CancellationToken ct)
    {
        await _taskListService.RenameAsync(userId, taskListId, request.Title, ct);
        return NoContent();
    }

    /// <summary>
    /// Removes task list from the system.
    /// </summary>
    [HttpDelete("{taskListId:guid}")]
    public async Task<IActionResult> Delete(
        [FromHeader]Guid userId,
        Guid taskListId,
        CancellationToken ct)
    {
        await _taskListService.DeleteAsync(userId, taskListId, ct);
        return NoContent();
    }

    /// <summary>
    /// Grants access to the task list for another user.
    /// </summary>
    [HttpPost("{taskListId:guid}/users/{targetUserId:guid}")]
    public async Task<IActionResult> AddUser(
        [FromHeader]Guid userId,
        Guid taskListId,
        Guid targetUserId,
        CancellationToken ct)
    {
        await _taskListService.AddUserAsync(userId, taskListId, targetUserId, ct);
        return NoContent();
    }

    /// <summary>
    /// Returns all users who have access to the task list.
    /// </summary>
    [HttpGet("{taskListId:guid}/users")]
    public async Task<IActionResult> GetUsers(
        [FromHeader]Guid userId,
        Guid taskListId,
        CancellationToken ct)
    {
        var userIds = await _taskListService.GetUsersAsync(userId, taskListId, ct);
        return Ok(new TaskListUsersDto(userIds));
    }

    /// <summary>
    /// Revokes user access from the task list.
    /// </summary>
    [HttpDelete("{taskListId:guid}/users/{targetUserId:guid}")]
    public async Task<IActionResult> RemoveUser(
        [FromHeader]Guid userId,
        Guid taskListId,
        Guid targetUserId,
        CancellationToken ct)
    {
        await _taskListService.RemoveUserAsync(userId, taskListId, targetUserId, ct);
        return NoContent();
    }
}
