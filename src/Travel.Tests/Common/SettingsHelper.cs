using Agents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TDD.Common.Settings;
using Travel.Tests.Evaluation;
using Travel.Tests.Integration;

namespace Travel.Tests.Common;

public static class SettingsHelper
{
    private const string LanguageModelSettingsDeploymentName = "LanguageModelSettings:DeploymentName";
    private const string LanguageModelSettingsEndpoint = "LanguageModelSettings:EndPoint";

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<PlanningWorkflowTests>()
            .Build();


    public static IOptions<LanguageModelSettings> GetLanguageModelSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TravelPlanning>()
            .Build();

        var deploymentName = configuration[LanguageModelSettingsDeploymentName];

        var endpoint = configuration[LanguageModelSettingsEndpoint];

        ArgumentException.ThrowIfNullOrEmpty(endpoint, LanguageModelSettingsEndpoint);
        ArgumentException.ThrowIfNullOrEmpty(deploymentName, LanguageModelSettingsDeploymentName);

        var languageModelSettings = Options.Create(new LanguageModelSettings
        {
            DeploymentName = deploymentName,
            EndPoint = endpoint,
        });

        return languageModelSettings;
    }

    public static IOptions<AspireDashboardSettings> GetAspireDashboardSettings()
    {
        var configuration = BuildConfiguration();

        var section = configuration.GetSection("AspireDashboard");

        var otlpEndpoint = section["OtlpEndpoint"];
        var otlpApiKey = section["OtlpApiKey"];

        ArgumentException.ThrowIfNullOrEmpty(otlpEndpoint, "AspireDashboard:OtlpEndpoint");
        ArgumentException.ThrowIfNullOrEmpty(otlpApiKey, "AspireDashboard:OtlpApiKey");

        return Options.Create(new AspireDashboardSettings
        {
            OtlpEndpoint = otlpEndpoint,
            OtlpApiKey = otlpApiKey,
        });
    }
}