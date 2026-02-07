using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Tests.Helpers;

public static class WorkflowEvents
{
    public static void MatchesAgentFunctionCallResponse(this RequestInfoEvent requestInfoEvent, InformationRequestDetails informationRequestDetails)
    {
        var data = requestInfoEvent.Data as ExternalRequest;

        Assert.NotNull(data);

        var request = data.Data.AsType(typeof(InformationRequest)) as InformationRequest;

        Assert.NotNull(request);

        Assert.Equal(request.Context, informationRequestDetails.Context);
        Assert.Equal(request.Entities, informationRequestDetails.Entities);
    }

}