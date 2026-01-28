using Agents.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Settings;
using System.Text.Json.Serialization;
using ServiceDefaults;
using Travel.Workflows.Api.Services;
using Travel.Workflows.Extensions;
using Travel.Workflows.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddWorkflowServices();
builder.Services.AddAgentServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<AzureStorageSettings>((options) => builder.Configuration.GetSection("AzureStorageSeedSettings").Bind(options));

builder.Services.AddHttpClient<ITravelService, TravelService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7010/");
});

builder.Services.AddSingleton<IAgentDiscoveryService, AgentDiscoveryService>();
builder.Services.AddSingleton<IWorkflowTaskManager, WorkflowTaskManager>();
builder.Services.AddSingleton<ITravelWorkflowService, TravelWorkflowService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var workflowService = app.Services.GetRequiredService<IWorkflowTaskManager>();

app.MapA2A(workflowService.TaskManager, "/api/a2a/travel");

app.Run();


