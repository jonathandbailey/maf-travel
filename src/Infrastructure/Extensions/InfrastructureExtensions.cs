using Application.Interfaces;
using Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IAzureStorageRepository, AzureStorageRepository>();
        services.AddScoped<ICheckpointRepository, CheckpointRepository>();

        services.AddScoped<IArtifactRepository, ArtifactRepository>();

        services.AddScoped<IWorkflowRepository, WorkflowRepository>();

        return services;
    }
}