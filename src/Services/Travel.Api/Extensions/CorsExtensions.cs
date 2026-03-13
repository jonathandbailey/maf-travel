namespace Travel.Api.Extensions;

public static class CorsExtensions
{
    private const string ServiceDiscoveryPolicyName = "CorsPolicyServiceDiscovery";
    private const string UiDevelopmentUrl = "http://localhost:5173";
   
    public static WebApplicationBuilder AddCorsPolicyFromServiceDiscovery(this WebApplicationBuilder builder)
    {
        

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(ServiceDiscoveryPolicyName, policy =>
            {
                policy.WithOrigins(UiDevelopmentUrl)
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