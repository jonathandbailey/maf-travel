using Agents.Services;
using Infrastructure.Settings;

namespace Travel.Experience.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureStorageSettings>(options => configuration.GetSection("AzureStorageSeedSettings").Bind(options));

        services.AddSingleton<IA2AAgentServiceDiscovery, A2AAgentServiceDiscovery>();

   
        return services;
    }
}