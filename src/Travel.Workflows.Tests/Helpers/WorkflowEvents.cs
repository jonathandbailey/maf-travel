using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using TravelPlanDto = Travel.Workflows.Dto.TravelPlanDto;

namespace Travel.Workflows.Tests.Helpers;

public static class WorkflowEvents
{
    public static void MatchesAgentFunctionCallResponse(this RequestInfoEvent requestInfoEvent, RequestInformationDto informationRequestDetails)
    {
        var data = requestInfoEvent.Data as ExternalRequest;

        Assert.NotNull(data);

        var request = data.Data.AsType(typeof(InformationRequest)) as InformationRequest;

        Assert.NotNull(request);

        Assert.Equal(request.Context, informationRequestDetails.Message);
        Assert.Equal(request.Entities, informationRequestDetails.RequiredInputs);
    }

    public static void MatchesAgentFunctionCallResponse(this TravelPlanUpdateEvent travelPlanUpdateEvent, TravelPlanDto travelPlanDto)
    {
        var data = travelPlanUpdateEvent.TravelPlanDto;

        Assert.NotNull(data);

        Assert.Equal(travelPlanDto.Origin, data.Origin);
        Assert.Equal(travelPlanDto.Destination, data.Destination);
        Assert.Equal(travelPlanDto.StartDate, data.StartDate);
        Assert.Equal(travelPlanDto.EndDate, data.EndDate);
    }

}