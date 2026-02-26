using Infrastructure.Interfaces;
using Infrastructure.Repository;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
  
        services.Configure<FileStorageSettings>(options => configuration.GetSection("FileStorageSettings").Bind(options));



        /*
        services.AddAzureClients(azure =>
        {
            azure.AddBlobServiceClient(configuration.GetConnectionString("blobs"));
        });

        services.AddSingleton<IAzureStorageRepository, AzureStorageRepository>();
        */
        services.AddSingleton<IArtifactRepository, ArtifactRepository2>();
        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();
        services.AddSingleton<IFileRepository, FileRepository>();
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();

        return services;
    }
}