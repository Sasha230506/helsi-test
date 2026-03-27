using Helsi.Application.Abstractions;
using Helsi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Helsi.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TaskListAccessPolicy>();
        services.AddScoped<ITaskListService, TaskListService>();
        return services;
    }
}