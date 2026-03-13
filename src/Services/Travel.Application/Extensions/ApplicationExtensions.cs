using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Travel.Application.Behaviors;
using Travel.Application.Features.TravelPlan.Commands;

namespace Travel.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateTravelPlanCommand>());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssemblyContaining<CreateTravelPlanCommand>();

        return services;
    }
}
