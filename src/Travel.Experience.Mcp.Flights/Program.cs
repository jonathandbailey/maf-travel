using ServiceDefaults;
using Travel.Experience.Mcp.Flights.Services;
using Travel.Experience.Mcp.Flights.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IFlightSearchService, StubFlightSearchService>();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<FlightSearchTools>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp("/mcp");

app.Run();
