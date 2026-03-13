using Microsoft.Extensions.DependencyInjection;
using Travel.Workflows.Infrastructure;
using Travel.Workflows.Interfaces;
using Travel.Workflows.Services;

namespace Travel.Workflows.Extensions;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowSessionRepository, WorkflowSessionRepository>();
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();

        services.AddHttpClient<ITravelApiClient, TravelApiClient>(client =>
            client.BaseAddress = new Uri("https+http://travel-api"));

        return services;
    }
}
