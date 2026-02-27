using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Travel.Experience.Application.Agents;
using Travel.Experience.Application.Agents.ToolHandling;

namespace Travel.Experience.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IToolHandler, TravelWorkflowToolHandler>();
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        services.AddSingleton<IConversationAgentFactory, ConversationAgentFactory>();

        return services;
    }
}