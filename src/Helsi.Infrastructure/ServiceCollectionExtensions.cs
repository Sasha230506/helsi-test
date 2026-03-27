using Helsi.Application.Abstractions;
using Helsi.Infrastructure.Persistence;
using Helsi.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Helsi.Infrastructure;

public static class ServiceCollectionExtensions
{
    private static bool _guidSerializerRegistered;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        RegisterMongoConventions();

        services.Configure<MongoDbSettings>(
            cfg.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<MongoIndexInitializer>();

        return services;
    }

    private static void RegisterMongoConventions()
    {
        if (_guidSerializerRegistered)
            return;

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        _guidSerializerRegistered = true;
    }
}