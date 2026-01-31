
using Agents.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using ServiceDefaults;
using Travel.Agents.A2A.Services;
using Travel.Agents.A2A.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<CardSettings>(settings =>
    builder.Configuration.GetSection(nameof(CardSettings)).Bind(settings));

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


app.Run();


