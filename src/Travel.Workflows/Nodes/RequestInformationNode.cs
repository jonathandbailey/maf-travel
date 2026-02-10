using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Nodes;

public class RequestInformationNode() : ReflectingExecutor<RequestInformationNode>("RequestInformation"), 
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
        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments["request"]);

        var details = JsonSerializer.Deserialize<RequestInformationDto>(argumentsJson, _serializerOptions);
        
        await context.SendMessageAsync(new InformationRequest(details.Message, details.RequiredInputs), cancellationToken: cancellationToken);
    }

    public async ValueTask HandleAsync(InformationResponse informationResponse, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await context.SendMessageAsync(informationResponse.Message, cancellationToken);
    }
}