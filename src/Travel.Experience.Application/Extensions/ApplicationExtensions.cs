using Agents.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Travel.Experience.Application.Agents;

namespace Travel.Experience.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IToolHandler, TravelWorkflowToolHandler>();
        services.AddSingleton<IToolRegistry>(sp => new ToolRegistry(sp.GetServices<IToolHandler>()));
        services.AddSingleton<IConversationAgentFactory, ConversationAgentFactory>();

        return services;
    }
}