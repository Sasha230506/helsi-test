namespace Helsi.Infrastructure.Persistence;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;

    public string TaskListsCollection { get; set; } = "task_lists";
}