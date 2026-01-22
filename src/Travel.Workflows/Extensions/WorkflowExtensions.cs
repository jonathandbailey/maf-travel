using Agents.Services;
using Microsoft.Extensions.DependencyInjection;
using Travel.Workflows.Repository;
using Travel.Workflows.Services;

namespace Travel.Workflows.Extensions;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();
        services.AddSingleton<IWorkflowRepository, WorkflowRepository>();

        services.AddSingleton<ITravelService, TravelService>();
        services.AddSingleton<IFlightService, FlightService>();

        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();

        services.AddSingleton<IA2AAgentServiceDiscovery, A2AAgentServiceDiscovery>();

        return services;
    }
}