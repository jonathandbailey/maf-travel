using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Travel.Experience.Application.Agents.ToolHandling;

namespace Travel.Experience.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureStorageSettings>(options => configuration.GetSection("AzureStorageSeedSettings").Bind(options));

        services.AddSingleton<IConversationToolHandler, TravelWorkflowToolHandler>();
        services.AddSingleton<IConversationToolHandlerRegistry, ConversationToolHandlerRegistry>();

        return services;
    }
}