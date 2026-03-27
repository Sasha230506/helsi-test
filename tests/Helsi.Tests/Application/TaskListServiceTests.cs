using Helsi.Application;
using Helsi.Application.Abstractions;
using Helsi.Application.Common;
using Helsi.Application.Services;
using Helsi.Domain.Entities;
using NSubstitute;

namespace Helsi.Tests.Application;

public sealed class TaskListServiceTests
{
    private ITaskListRepository _repo = null!;
    private TaskListAccessPolicy _policy = null!;
    private TaskListService _service = null!;
    private CancellationToken _ct;

    [SetUp]
    public void SetUp()
    {
        _repo = Substitute.For<ITaskListRepository>();
        _policy = new TaskListAccessPolicy(_repo);
        _service = new TaskListService(_repo, _policy);
        _ct = CancellationToken.None;
    }

    [Test]
    public async Task Create_ValidInput_ReturnsIdAndCallsRepo()
    {
        var userId = Guid.NewGuid();

        var id = await _service.CreateAsync(userId, "Shopping", _ct);

        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        await _repo.Received(1).AddAsync(Arg.Is<TaskList>(t => t.Title == "Shopping" && t.OwnerId == userId), _ct);
    }

    [Test]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), _ct).Returns((TaskList?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid(), _ct);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetById_Owner_ReturnsList()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        var result = await _service.GetByIdAsync(ownerId, list.Id, _ct);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(list.Id));
    }

    [Test]
    public void GetById_NoAccess_ThrowsUnauthorized()
    {
        var list = new TaskList(Guid.NewGuid(), "List");
        var stranger = Guid.NewGuid();
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);
        _repo.IsUserLinkedAsync(list.Id, stranger, _ct).Returns(false);

        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.GetByIdAsync(stranger, list.Id, _ct));
    }

    [Test]
    public void Rename_NotFound_ThrowsKeyNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), _ct).Returns((TaskList?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RenameAsync(Guid.NewGuid(), Guid.NewGuid(), "New", _ct));
    }

    [Test]
    public void Rename_NoAccess_ThrowsUnauthorized()
    {
        var list = new TaskList(Guid.NewGuid(), "Old");
        var stranger = Guid.NewGuid();
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);
        _repo.IsUserLinkedAsync(list.Id, stranger, _ct).Returns(false);

        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.RenameAsync(stranger, list.Id, "New", _ct));
    }

    [Test]
    public async Task Rename_Owner_UpdatesTitle()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "Old");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        await _service.RenameAsync(ownerId, list.Id, "New Title", _ct);

        await _repo.Received(1).UpdateAsync(Arg.Is<TaskList>(t => t.Title == "New Title"), _ct);
    }

    [Test]
    public void Delete_NotFound_ThrowsKeyNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), _ct).Returns((TaskList?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), _ct));
    }

    [Test]
    public void Delete_NonOwner_ThrowsUnauthorized()
    {
        var list = new TaskList(Guid.NewGuid(), "List");
        var stranger = Guid.NewGuid();
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteAsync(stranger, list.Id, _ct));
    }

    [Test]
    public async Task Delete_Owner_RemovesList()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        await _service.DeleteAsync(ownerId, list.Id, _ct);

        await _repo.Received(1).RemoveAsync(list.Id, _ct);
    }

    [Test]
    public void AddUser_NoAccess_ThrowsUnauthorized()
    {
        var list = new TaskList(Guid.NewGuid(), "List");
        var stranger = Guid.NewGuid();
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);
        _repo.IsUserLinkedAsync(list.Id, stranger, _ct).Returns(false);

        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.AddUserAsync(stranger, list.Id, Guid.NewGuid(), _ct));
    }

    [Test]
    public void AddUser_TargetIsOwner_ThrowsInvalidOperation()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddUserAsync(ownerId, list.Id, ownerId, _ct));
    }

    [Test]
    public void AddUser_AlreadyLinked_ThrowsInvalidOperation()
    {
        var ownerId = Guid.NewGuid();
        var target = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);
        _repo.IsUserLinkedAsync(list.Id, target, _ct).Returns(true);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddUserAsync(ownerId, list.Id, target, _ct));
    }

    [Test]
    public async Task AddUser_Valid_AddsLink()
    {
        var ownerId = Guid.NewGuid();
        var target = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);
        _repo.IsUserLinkedAsync(list.Id, target, _ct).Returns(false);

        await _service.AddUserAsync(ownerId, list.Id, target, _ct);

        await _repo.Received(1).AddUserLinkAsync(list.Id, target, _ct);
    }

    [Test]
    public void RemoveUser_TargetIsOwner_ThrowsInvalidOperation()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RemoveUserAsync(ownerId, list.Id, ownerId, _ct));
    }

    [Test]
    public async Task RemoveUser_Valid_RemovesLink()
    {
        var ownerId = Guid.NewGuid();
        var target = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);

        await _service.RemoveUserAsync(ownerId, list.Id, target, _ct);

        await _repo.Received(1).RemoveUserLinkAsync(list.Id, target, _ct);
    }

    [Test]
    public async Task GetUsers_Owner_ReturnsLinkedUsers()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");
        var linked = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _repo.GetByIdAsync(list.Id, _ct).Returns(list);
        _repo.GetLinkedUsersAsync(list.Id, _ct).Returns(linked);

        var result = await _service.GetUsersAsync(ownerId, list.Id, _ct);

        Assert.That(result, Is.EqualTo(linked));
    }

    [Test]
    public async Task GetPaged_DelegatesToRepo()
    {
        var userId = Guid.NewGuid();
        var paged = new PagedResult<TaskList>([], 0, 1, 20);
        _repo.GetForUserAsync(userId, 1, 20, _ct).Returns(paged);

        var result = await _service.GetPagedAsync(userId, 1, 20, _ct);

        Assert.That(result.Total, Is.EqualTo(0));
    }

    [Test]
    public async Task GetPaged_NormalizesInvalidPage()
    {
        var userId = Guid.NewGuid();
        var paged = new PagedResult<TaskList>([], 0, 1, 20);
        _repo.GetForUserAsync(userId, 1, 20, _ct).Returns(paged);

        var result = await _service.GetPagedAsync(userId, -5, 20, _ct);

        Assert.That(result.Page, Is.EqualTo(1));
    }
}
