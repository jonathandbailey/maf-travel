namespace Api.Extensions;

public static class CorsExtensions
{
    // TODO : Should move to configuration?
    private const string ServiceDiscoveryPolicyName = "CorsPolicyServiceDiscovery";
    private const string UiDevelopmentUrl = "http://localhost:5173";
    private const string ServicesConfigKey = "services:ui:Http:0";

    public static WebApplicationBuilder AddCorsPolicyFromServiceDiscovery(this WebApplicationBuilder builder)
    {
        var url = builder.Configuration[ServicesConfigKey];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("Service Discovery Key : services:ui:Http:0, not found");
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(ServiceDiscoveryPolicyName, policy =>
            {
                policy.WithOrigins(url,UiDevelopmentUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();

            });
        });

        return builder;
    }

    public static WebApplication UseCorsPolicyServiceDiscovery(this WebApplication app)
    {
        app.UseCors(ServiceDiscoveryPolicyName);

        return app;
    }
}