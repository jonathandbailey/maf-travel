using Microsoft.Extensions.DependencyInjection;
using Travel.Agents.Services;

namespace Travel.Agents.Extensions;

public static class TravelAgentExtensions
{
    public static IServiceCollection AddTravelAgentServices(this IServiceCollection services)
    {
        services.AddSingleton<IAgentProvider, AgentProvider>();

        return services;
    }
}
