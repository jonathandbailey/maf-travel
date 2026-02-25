using Travel.Tests.Shared.Helper;
using Travel.Tests.Shared.Settings;

namespace Travel.Tests.Shared;

public class TelemetryFixture : IDisposable
{
    public TelemetryFixture()
    {
        TelemetryHelper.Initialize(SettingsHelper.GetAspireDashboardSettings());
    }

    public void Dispose()
    {
        TelemetryHelper.Dispose();
    }
}
