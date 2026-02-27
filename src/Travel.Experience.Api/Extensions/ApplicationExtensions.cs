using Travel.Experience.Application.Agents.ToolHandling;

namespace Travel.Experience.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IToolHandler, TravelWorkflowToolHandler>();
        services.AddSingleton<IToolRegistry, ToolRegistry>();

        return services;
    }
}