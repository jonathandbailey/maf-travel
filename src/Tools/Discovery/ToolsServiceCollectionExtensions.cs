using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tools.Registry;

namespace Tools.Discovery;

public static class ToolsServiceCollectionExtensions
{
    public static IServiceCollection AddTools(
        this IServiceCollection services,
        params Type[] markerTypes) =>
        services.AddTools(markerTypes.Select(t => t.Assembly).Distinct().ToArray());

    private static IServiceCollection AddTools(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var handlerInterface = typeof(IToolHandler);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && handlerInterface.IsAssignableFrom(t)))
            {
                var groups = type.GetCustomAttributes<ToolGroupAttribute>()
                    .Select(a => a.Group)
                    .ToList();

                services.AddTransient(type);
                services.AddSingleton(new ToolHandlerDescriptor(type, groups));
            }
        }

        services.AddSingleton<IToolRegistry>(sp =>
        {
            var registrations = sp.GetServices<ToolHandlerDescriptor>()
                .Select(d => new ToolHandlerRegistration(
                    (IToolHandler)sp.GetRequiredService(d.HandlerType),
                    d.Groups));

            return new ToolRegistry(registrations);
        });

        return services;
    }
}
