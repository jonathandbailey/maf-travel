using Agents.Tools;
using Microsoft.Extensions.DependencyInjection;
using Travel.Experience.Application.Agents;

namespace Travel.Experience.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<TravelWorkflowToolHandler>();
        services.AddSingleton<IToolRegistry>(sp =>
        {
            var handler = sp.GetRequiredService<TravelWorkflowToolHandler>();
            return new ToolRegistry([new ToolHandlerRegistration(handler, ["conversation"])]);
        });
        services.AddSingleton<IConversationAgentFactory, ConversationAgentFactory>();

        return services;
    }
}