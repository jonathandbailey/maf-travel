using Agents.Tools;
using Microsoft.Extensions.DependencyInjection;
using Travel.Agents.Services;
using Travel.Experience.Application.Agents;
using Travel.Workflows.Common.Interfaces;
using Travel.Workflows.Flights.Services;

namespace Travel.Experience.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<TravelWorkflowToolHandler>(sp =>
            new TravelWorkflowToolHandler(() => sp.GetRequiredService<IWorkflowFactory>()));
        services.AddSingleton<FlightsWorkflowToolHandler>(sp =>
            new FlightsWorkflowToolHandler(() => sp.GetRequiredService<FlightsWorkflowService>()));
        services.AddSingleton<PlanningToolsHandler>();
        services.AddSingleton<ExtractingToolsHandler>();
        services.AddSingleton<FlightSearchToolsHandler>();
        services.AddSingleton<IToolRegistry>(sp =>
        {
            var conversation = sp.GetRequiredService<TravelWorkflowToolHandler>();
            var flightsWorkflow = sp.GetRequiredService<FlightsWorkflowToolHandler>();
            var planning = sp.GetRequiredService<PlanningToolsHandler>();
            var extracting = sp.GetRequiredService<ExtractingToolsHandler>();
            var flightSearch = sp.GetRequiredService<FlightSearchToolsHandler>();
            return new ToolRegistry(
            [
                new ToolHandlerRegistration(conversation, ["conversation"]),
                new ToolHandlerRegistration(flightsWorkflow, ["conversation"]),
                new ToolHandlerRegistration(planning, ["planning"]),
                new ToolHandlerRegistration(extracting, ["extracting"]),
                new ToolHandlerRegistration(flightSearch, ["flight"])
            ]);
        });
        services.AddSingleton<IConversationAgentFactory, ConversationAgentFactory>();

        return services;
    }
}