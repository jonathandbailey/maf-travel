using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;

namespace AppHost.Extensions;

public static class UiServiceExtensions
{
    private const string UiName = "ui";
    private const string UiSourcePath = "../../ui";
    private const string UiScriptName = "dev";
    private const string UiViteApiBaseUrl = "VITE_API_BASE_URL";
    private const string UiVitePort = "VITE_PORT";
    private const int UiPort = 5173;
    private const string UiEndPointReference = "https";

    public static IResourceBuilder<JavaScriptAppResource> AddUiServices(
        this IDistributedApplicationBuilder builder, 
        IResourceBuilder<ProjectResource> api)
    {
        var ui = builder.AddJavaScriptApp(UiName, UiSourcePath, UiScriptName)
            .WithReference(api)

            .WaitFor(api)
            .WithEnvironment(UiViteApiBaseUrl, api.GetEndpoint(UiEndPointReference))
            .WithHttpEndpoint(port: UiPort, env: UiVitePort)
            .WithExternalHttpEndpoints();

        return ui;
    }
}