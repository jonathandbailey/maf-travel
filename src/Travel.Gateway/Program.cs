using ServiceDefaults;

using Travel.Experience.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddCorsPolicyFromServiceDiscovery();


builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapReverseProxy();

app.UseCorsPolicyServiceDiscovery();

app.Run();

