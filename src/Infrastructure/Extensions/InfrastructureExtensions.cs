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

        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();
        services.AddSingleton<IFileRepository, FileRepository>();
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();
        services.AddSingleton<IAgentThreadRepository, AgentThreadRepository>();

        return services;
    }
}