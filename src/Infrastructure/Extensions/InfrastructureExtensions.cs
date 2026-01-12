using Infrastructure.Interfaces;
using Infrastructure.Repository;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureClients(azure =>
        {
            azure.AddBlobServiceClient(configuration.GetConnectionString("blobs"));
        });

        services.AddSingleton<IAzureStorageRepository, AzureStorageRepository>();
   
        services.AddSingleton<IArtifactRepository, ArtifactRepository>();

        return services;
    }
}