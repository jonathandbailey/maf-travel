using Infrastructure.Settings;
using Travel.Experience.Application.Agents.ToolHandling;

namespace Travel.Experience.Api.Extensions;

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