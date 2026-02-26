using Agents.Extensions;
using Infrastructure.Extensions;
using ServiceDefaults;
using Travel.Agents.Extensions;
using Travel.Experience.Api.Extensions;
using Travel.Workflows.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddAgentServices(builder.Configuration);

builder.Services.AddTravelAgentServices();

builder.Services.AddWorkflowServices();

builder.Services.AddApplicationServices(builder.Configuration);

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

await app.MapAgUiToAgent();




app.Run();
