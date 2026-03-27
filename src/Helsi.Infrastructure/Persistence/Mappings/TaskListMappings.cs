using Helsi.Domain.Entities;
using Helsi.Infrastructure.Persistence.Documents;

namespace Helsi.Infrastructure.Persistence.Mappings;

public static class TaskListMappings
{
    public static TaskListDocument ToDocument(this TaskList entity, IReadOnlyCollection<Guid>? sharedWith = null)
    {
        return new TaskListDocument
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Title = entity.Title,
            CreatedAtUtc = entity.CreatedAtUtc,
            SharedWithUserIds = sharedWith?.ToList() ?? []
        };
    }

    public static TaskList ToDomain(this TaskListDocument document)
    {
        return TaskList.Restore(
            document.Id,
            document.OwnerId,
            document.Title,
            document.CreatedAtUtc);
    }
}