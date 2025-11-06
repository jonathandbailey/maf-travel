using ConsoleApp;
using ConsoleApp.Services;
using ConsoleApp.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFiles(hostingContext.HostingEnvironment);
    config.AddEnvironmentVariables();
});

builder.ConfigureServices((context, services) =>
{
    services.AddHttpClient();

    services.Configure<ChatClientSetting>(context.Configuration.GetSection("ChatClientSettings"));

    services.AddSingleton<IChatClient, ChatClient>();
    services.AddSingleton<ConsoleApplication>();
    services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
});

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
});

var host = builder.Build();

using var cancellationTokenSource = new CancellationTokenSource();

await host.Services.GetRequiredService<ConsoleApplication>().RunAsync(cancellationTokenSource);