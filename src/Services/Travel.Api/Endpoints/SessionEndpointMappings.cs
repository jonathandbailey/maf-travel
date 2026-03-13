using MediatR;
using Travel.Application.Features.Session.Commands;

namespace Travel.Api.Endpoints;

public class SessionEndpointMappings : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sessions");
        group.MapPost("", CreateAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender)
    {
        var response = await sender.Send(new CreateSessionCommand());
        return Results.Created($"/sessions/{response.Id}", response);
    }
}
