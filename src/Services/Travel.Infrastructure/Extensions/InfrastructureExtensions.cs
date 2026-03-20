using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Travel.Application.Interfaces;
using Travel.Infrastructure.Queries;
using Travel.Infrastructure.Repositories;

namespace Travel.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddTravelInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TravelPlanStorageSettings>(options =>
            configuration.GetSection("TravelPlanStorageSettings").Bind(options));

        services.Configure<SessionStorageSettings>(options =>
            configuration.GetSection("SessionStorageSettings").Bind(options));

        services.Configure<FlightSearchStorageSettings>(options =>
            configuration.GetSection("FlightSearchStorageSettings").Bind(options));

        services.AddAzureClients(azure =>
            azure.AddBlobServiceClient(configuration.GetConnectionString("blobs")));

        services.AddSingleton<IAzureStorageRepository, AzureStorageRepository>();
        services.AddSingleton<ITravelPlanRepository, TravelPlanRepository>();
        services.AddSingleton<ISessionRepository, SessionRepository>();
        services.AddSingleton<IFlightSearchRepository, FlightSearchRepository>();

        services.AddSingleton<ITravelPlanQuery, TravelPlanQuery>();

        return services;
    }
}
