using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Travel.Tests.Shared.Settings;

namespace Travel.Tests.Shared.Helper;

public static class TelemetryHelper
{
    private static TracerProvider? _tracerProvider;
    private static MeterProvider? _meterProvider;

    public static void Initialize(IOptions<AspireDashboardSettings> settings)
    {
        var dashboardSettings = settings.Value;

        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("Travel.Tests"))
            .AddSource("Travel.Workflows*")
            .AddSource("Travel.Tests*")
            .AddSource("Travel.Experience*")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(dashboardSettings.OtlpEndpoint);
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                options.Headers = $"x-otlp-api-key={dashboardSettings.OtlpApiKey}";
            })
            .Build();

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("Travel.Tests"))
            .AddMeter("Application.Workflows")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(dashboardSettings.OtlpEndpoint);
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                options.Headers = $"x-otlp-api-key={dashboardSettings.OtlpApiKey}";
            })
            .Build();
    }

    public static void Dispose()
    {
        _tracerProvider?.Dispose();
        _meterProvider?.Dispose();
    }
}
