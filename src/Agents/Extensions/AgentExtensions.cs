using Agents.Middleware;
using Agents.Services;
using Agents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agents.Extensions;

public static class AgentExtensions
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageModelSettings>(settings =>
            configuration.GetSection("LanguageModelSettings").Bind(settings));


        services.AddSingleton<IAgentMemoryService, AgentMemoryService>();
        services.AddSingleton<IAgentMemoryMiddleware, AgentMemoryMiddleware>();

        return services;
    }
}