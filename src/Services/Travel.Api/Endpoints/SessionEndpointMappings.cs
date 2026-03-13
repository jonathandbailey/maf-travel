using MediatR;
using Travel.Application.Features.Session.Commands;

namespace Travel.Api.Endpoints;

public class SessionEndpointMappings : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sessions");
        group.MapPost("", CreateAsync);
        group.MapPatch("/{id:guid}", UpdateAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender)
    {
        var response = await sender.Send(new CreateSessionCommand());
        return Results.Created($"/sessions/{response.Id}", response);
    }

    private static async Task<IResult> UpdateAsync(ISender sender, Guid id, UpdateSessionRequest request)
    {
        var response = await sender.Send(new UpdateSessionCommand(id, request.TravelPlanId));
        return Results.Ok(response);
    }
}

public record UpdateSessionRequest(Guid TravelPlanId);
