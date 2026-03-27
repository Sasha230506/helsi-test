using Helsi.Application;
using Helsi.Application.Abstractions;
using Helsi.Domain.Entities;
using NSubstitute;

namespace Helsi.Tests.Application;

public sealed class TaskListAccessPolicyTests
{
    private ITaskListRepository _repo = null!;
    private TaskListAccessPolicy _policy = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = Substitute.For<ITaskListRepository>();
        _policy = new TaskListAccessPolicy(_repo);
    }

    [Test]
    public async Task CanAccess_Owner_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");

        var result = await _policy.CanAccessAsync(list, ownerId, CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CanAccess_LinkedUser_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var linkedUserId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");

        _repo.IsUserLinkedAsync(list.Id, linkedUserId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _policy.CanAccessAsync(list, linkedUserId, CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CanAccess_UnrelatedUser_ReturnsFalse()
    {
        var list = new TaskList(Guid.NewGuid(), "List");
        var stranger = Guid.NewGuid();

        _repo.IsUserLinkedAsync(list.Id, stranger, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _policy.CanAccessAsync(list, stranger, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CanDelete_Owner_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var list = new TaskList(ownerId, "List");

        var result = await _policy.CanDeleteAsync(list, ownerId, CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CanDelete_NonOwner_ReturnsFalse()
    {
        var list = new TaskList(Guid.NewGuid(), "List");

        var result = await _policy.CanDeleteAsync(list, Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.False);
    }
}
