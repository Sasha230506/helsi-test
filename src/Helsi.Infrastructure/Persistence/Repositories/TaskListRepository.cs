using Helsi.Application.Abstractions;
using Helsi.Application.Common;
using Helsi.Domain.Entities;
using Helsi.Infrastructure.Persistence.Documents;
using Helsi.Infrastructure.Persistence.Mappings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Helsi.Infrastructure.Persistence.Repositories;

public sealed class TaskListRepository : ITaskListRepository
{
    private readonly IMongoCollection<TaskListDocument> _collection;

    public TaskListRepository(IMongoDatabase database, IOptions<MongoDbSettings> settings)
    {
        _collection = database.GetCollection<TaskListDocument>(settings.Value.TaskListsCollection);
    }

    public async Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var document = await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(ct);

        return document?.ToDomain();
    }

    public async Task<PagedResult<TaskList>> GetForUserAsync(Guid userId, int page, int size, CancellationToken ct)
    {
        var filter = Builders<TaskListDocument>.Filter.Or(
            Builders<TaskListDocument>.Filter.Eq(x => x.OwnerId, userId),
            Builders<TaskListDocument>.Filter.AnyEq(x => x.SharedWithUserIds, userId)
        );

        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync(ct);

        return new PagedResult<TaskList>(
            items.Select(x => x.ToDomain()).ToList(),
            total,
            page,
            size);
    }

    public async Task AddAsync(TaskList entity, CancellationToken ct)
    {
        var document = entity.ToDocument();
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(TaskList entity, CancellationToken ct)
    {
        var update = Builders<TaskListDocument>.Update.Set(x => x.Title, entity.Title);

        var result = await _collection.UpdateOneAsync(
            x => x.Id == entity.Id,
            update,
            cancellationToken: ct);

        if (result.MatchedCount == 0)
            throw new KeyNotFoundException($"Task list with id {entity.Id} was not found.");
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, ct);
    }

    public async Task<bool> IsUserLinkedAsync(Guid listId, Guid userId, CancellationToken ct)
    {
        var filter = Builders<TaskListDocument>.Filter.And(
            Builders<TaskListDocument>.Filter.Eq(x => x.Id, listId),
            Builders<TaskListDocument>.Filter.AnyEq(x => x.SharedWithUserIds, userId)
        );

        return await _collection.Find(filter).AnyAsync(ct);
    }

    public async Task AddUserLinkAsync(Guid listId, Guid userId, CancellationToken ct)
    {
        var update = Builders<TaskListDocument>.Update.AddToSet(x => x.SharedWithUserIds, userId);

        await _collection.UpdateOneAsync(
            x => x.Id == listId,
            update,
            cancellationToken: ct);
    }

    public async Task RemoveUserLinkAsync(Guid listId, Guid userId, CancellationToken ct)
    {
        var update = Builders<TaskListDocument>.Update.Pull(x => x.SharedWithUserIds, userId);

        await _collection.UpdateOneAsync(
            x => x.Id == listId,
            update,
            cancellationToken: ct);
    }

    public async Task<IReadOnlyList<Guid>> GetLinkedUsersAsync(Guid listId, CancellationToken ct)
    {
        var users = await _collection
            .Find(x => x.Id == listId)
            .Project(x => x.SharedWithUserIds)
            .FirstOrDefaultAsync(ct);

        return users ?? [];
    }
}