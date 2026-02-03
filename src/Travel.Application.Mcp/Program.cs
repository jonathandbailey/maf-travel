using Infrastructure.Extensions;
using Infrastructure.Settings;
using ServiceDefaults;
using Travel.Application.Extensions;
using Travel.Application.Mcp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(FlightTools).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(ApplicationServicesExtensions).Assembly);
});

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<AzureStorageSettings>((options) => builder.Configuration.GetSection("AzureStorageSeedSettings").Bind(options));


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:6274") // The Inspector's default port
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("mcp-session-id", "Mcp-Session-Id");
    });
});

builder.Services.AddApplicationServices();

builder.Services
.AddMcpServer().
WithHttpTransport().
WithTools<FlightTools>()
.WithTools<TravelPlanTools>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.MapMcp();

app.Run();


