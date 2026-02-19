using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class InformationRequestNode() : Executor<FunctionCallContent>(NodeNames.InformationRequestNode) 
{
    public override async ValueTask HandleAsync(FunctionCallContent functionCallContent, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.InformationRequestNode, Guid.NewGuid());

        if (functionCallContent.TryGetArgument<RequestInformationDto>(WorkflowConstants.InformationRequestFunctionArgumentName, out var details, Json.FunctionCallSerializerOptions))
        {
            await context.SendMessageAsync(new InformationRequest(details.Message, details.RequiredInputs),
                cancellationToken: cancellationToken);
        }
    }
}