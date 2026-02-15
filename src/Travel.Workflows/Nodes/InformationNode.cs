using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Travel.Agents.Dto;
using Travel.Workflows.Dto;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class InformationNode() : ReflectingExecutor<InformationNode>("Information"), 
    IMessageHandler<FunctionCallContent>,
    IMessageHandler<InformationResponse>
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask HandleAsync(FunctionCallContent functionCallContent, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("Information", Guid.NewGuid());


        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments["request"]);

        var details = JsonSerializer.Deserialize<RequestInformationDto>(argumentsJson, _serializerOptions);
        
        await context.SendMessageAsync(new InformationRequest(details.Message, details.RequiredInputs), cancellationToken: cancellationToken);
    }

    public async ValueTask HandleAsync(InformationResponse informationResponse, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("Information", Guid.NewGuid());

        await context.SendMessageAsync(informationResponse.Message, cancellationToken);
    }
}