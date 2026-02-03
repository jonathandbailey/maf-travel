using MediatR;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Travel.Application.Api.Dto;
using Travel.Application.Application.Commands;
using Travel.Application.Application.Queries;
using Travel.Application.Mcp.Observability;

namespace Travel.Application.Mcp.Tools;

[McpServerToolType]
public class TravelPlanTools(IMediator mediator)
{
    [McpServerTool]
    [Description("Get a travel plan by ID")]
    public async Task<TravelPlanDto> GetTravelPlan(
       [Description("The user ID")] Guid userId,
       [Description("The travel plan ID")] Guid travelPlanId)
    {
        var result = await mediator.Send(new GetTravelPlanQuery(userId, travelPlanId));
        return result;
    }

    [McpServerTool]
    [Description("Update a travel plan with new details")]
    public async Task UpdateTravelPlan(
       [Description("The user ID")] Guid userId,
       [Description("The session ID")] Guid sessionId,
       [Description("The travel plan update data")] TravelPlanUpdateDto travelPlanUpdateDto)
    {
        using var activity = TravelMcpTelemetry.UpdateTravelPlan();
        
        await mediator.Send(new UpdateTravelPlanCommand(userId, sessionId, travelPlanUpdateDto));
    }
}
