using Agents.Middleware;
using Agents.Services;
using Agents.Settings;
using Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agents.Extensions;

public static  class AgentServiceExtensions
{
    public static IServiceCollection AddAgentServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageModelSettings>(settings =>
            configuration.GetSection("LanguageModelSettings").Bind(settings));


        services.AddSingleton<IAgentMemoryService, AgentMemoryService>();
        services.AddSingleton<IAgentThreadMiddleware, AgentThreadMiddleware>();
        services.AddSingleton<IAgentAgUiMiddleware, AgentAgUiMiddleware>();
        services.AddSingleton<IAgentFactory, AgentFactory>();
        services.AddSingleton<IAgentMiddlewareFactory, AgentMiddlewareFactory>();
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();

        return services;
    }
}