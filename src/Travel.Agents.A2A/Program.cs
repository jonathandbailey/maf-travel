
using Agents.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using ServiceDefaults;
using Travel.Agents.A2A.Flights.Services;
using Travel.Agents.A2A.Planning.Extensions;
using Travel.Agents.A2A.Planning.Services;
using Travel.Agents.A2A.Settings;
using Travel.Agents.A2A.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<CardSettings>(settings =>
    builder.Configuration.GetSection(nameof(CardSettings)).Bind(settings));

builder.Services.Configure<ServerSettings>(settings =>
{
    settings.ServiceUrl = builder.Configuration["services:travel-application-mcp:https:0"]
                          ?? throw new InvalidOperationException("MCP service URL not configured");
});

builder.Services.AddPlanningServices();

builder.Services.AddHttpClient<IMcpToolsService, McpToolsService>();

builder.Services.AddSingleton<IMcpToolsService, McpToolsService>();

builder.Services.AddSingleton<IFlightsTaskManager, FlightsTaskManager>();
builder.Services.AddSingleton<IFlightService, FlightService>();
builder.Services.AddSingleton<IA2ACardService, A2ACardService>();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAgentServices(builder.Configuration);

builder.Services.AddOpenApi();



var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var workflowService = app.Services.GetRequiredService<IFlightsTaskManager>();

var cardSettings = app.Services.GetRequiredService<IOptions<CardSettings>>();

const string Flights = "Flights";
const string? FlightsAgentCardConfigurationNotFound = "Flights agent card configuration not found";


var agentCard = cardSettings.Value.AgentCards.First(c => c.Name == Flights)
                ?? throw new InvalidOperationException(FlightsAgentCardConfigurationNotFound);

app.MapA2A(workflowService.TaskManager, $"{agentCard.Url}");


var planning = app.Services.GetRequiredService<IPlanningTaskManager>();


const string Planning = "Planning";
const string? PlanningAgentCardConfigurationNotFound = "Planning agent card configuration not found";


var planningCard = cardSettings.Value.AgentCards.First(c => c.Name == Planning)
                ?? throw new InvalidOperationException(PlanningAgentCardConfigurationNotFound);

app.MapA2A(planning.TaskManager, $"{planningCard.Url}");


app.Run();


