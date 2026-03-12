using ServiceDefaults;
using Travel.Api;
using Travel.Application.Extensions;
using Travel.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddTravelInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

foreach (var endpoint in typeof(IEndpoint).Assembly.GetTypes()
    .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpoint)))
    .Select(Activator.CreateInstance)
    .Cast<IEndpoint>())
{
    endpoint.MapRoutes(app);
}

app.Run();

