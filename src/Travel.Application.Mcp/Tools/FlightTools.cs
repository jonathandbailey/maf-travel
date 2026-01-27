using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using Travel.Application.Api.Dto;
using Travel.Application.Application.Queries;

namespace Travel.Application.Mcp.Tools;

[McpServerToolType]
public class FlightTools(IMediator mediator)
{
    [McpServerTool]
    [Description("Get a flight search by ID")]
    public async Task<FlightSearchResultDto> GetFlightSearch(
       [Description("The Id of the flight search")] Guid id)
    {
        var result = await mediator.Send(new GetFlightSearchQuery(id));
        return result;
    }
}