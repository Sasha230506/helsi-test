using MongoDB.Bson.Serialization.Attributes;

namespace Helsi.Infrastructure.Persistence.Documents;

public sealed class TaskListDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public List<Guid> SharedWithUserIds { get; set; } = [];
}