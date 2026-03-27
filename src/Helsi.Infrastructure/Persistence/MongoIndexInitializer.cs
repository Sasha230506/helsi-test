using Helsi.Infrastructure.Persistence.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Helsi.Infrastructure.Persistence;

public sealed class MongoIndexInitializer
{
    private readonly IMongoCollection<TaskListDocument> _collection;

    public MongoIndexInitializer(IMongoDatabase database, IOptions<MongoDbSettings> settings)
    {
        var collectionName = settings.Value.TaskListsCollection;
        _collection = database.GetCollection<TaskListDocument>(collectionName);
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var models = new List<CreateIndexModel<TaskListDocument>>
        {
            new(Builders<TaskListDocument>.IndexKeys.Ascending(x => x.OwnerId)),
            new(Builders<TaskListDocument>.IndexKeys.Descending(x => x.CreatedAtUtc)),
            new(Builders<TaskListDocument>.IndexKeys.Ascending(x => x.SharedWithUserIds))
        };

        await _collection.Indexes.CreateManyAsync(models, cancellationToken: ct);
    }
}