using Agents.Extensions;
using Infrastructure.Extensions;
using ServiceDefaults;
using Travel.Agents.Extensions;
using Travel.Experience.Api;
using Travel.Experience.Api.Extensions;
using Travel.Experience.Application.Extensions;
using Travel.Workflows.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddCorsPolicyFromServiceDiscovery();


builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAgentServices(builder.Configuration);

builder.Services.AddTravelAgentServices();

builder.Services.AddWorkflowServices();

builder.Services.AddApplicationServices();

builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCorsPolicyServiceDiscovery();

await app.MapAgUiToAgent();

app.MapTravelPlanEndpoints();





app.Run();
