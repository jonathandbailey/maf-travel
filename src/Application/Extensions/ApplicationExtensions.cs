using Application.Agents;
using Application.Agents.Repository;
using Application.Services;
using Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageModelSettings>(settings => 
            configuration.GetSection("LanguageModelSettings").Bind(settings));

        services.AddScoped<IAgentFactory, AgentFactory>();
        services.AddScoped<IAgentTemplateRepository, AgentTemplateRepository>();
        services.AddScoped<IApplicationService, ApplicationService>();
        
        return services;
    }
}