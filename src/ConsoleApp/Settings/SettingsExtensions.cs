using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp.Settings;

public static class SettingsExtensions
{
    public static void AddJsonFiles(this IConfigurationBuilder configurationBuilder, IHostEnvironment env)
    {
        configurationBuilder.AddJsonFile(Constants.AppSettings, optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile(string.Format(Constants.AppSettingsDevelopment, env.EnvironmentName), optional: false, reloadOnChange: true);
    }
}