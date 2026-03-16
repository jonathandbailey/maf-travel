using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using Travel.Workflows.Flights.Services;
using Travel.Workflows.Infrastructure;
using Travel.Workflows.Interfaces;
using Travel.Workflows.TravelPlanCriteria.Services;

namespace Travel.Workflows.Extensions;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowSessionRepository, WorkflowSessionRepository>();
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();

        services.AddHttpClient<ITravelApiClient, TravelApiClient>(client =>
            client.BaseAddress = new Uri("https+http://travel-api"));

        services.AddHttpClient<IFlightApiClient, FlightApiClient>(client =>
            client.BaseAddress = new Uri("https+http://travel-api"));

        services.AddHttpClient("mcp-flights");

        services.AddSingleton<McpClient>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("mcp-flights");
            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri("https://travel-experience-mcp-flights/mcp"),
                Name = "Flights MCP Client"
            }, httpClient);
            return McpClient.CreateAsync(transport).GetAwaiter().GetResult();
        });

        services.AddSingleton<FlightsWorkflowService>();

        return services;
    }
}
