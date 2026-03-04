using Infrastructure.Repository;
using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
  
        services.Configure<FileStorageSettings>(options => configuration.GetSection("FileStorageSettings").Bind(options));
        services.Configure<AzureStorageSettings>(options => configuration.GetSection("AzureStorageSettings").Bind(options));

        services.AddAzureClients(azure =>
        {
            azure.AddBlobServiceClient(configuration.GetConnectionString("blobs"));
        });

        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();
        services.AddSingleton<IFileRepository, FileRepository>();
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();
        services.AddSingleton<IAgentThreadRepository, AzureStorageAgentThreadRepository>();
        services.AddSingleton<IAzureStorageRepository, AzureStorageRepository>();

        return services;
    }
}