using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Planning.Nodes;

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
        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments["informationRequest"]);

        var details = JsonSerializer.Deserialize<InformationRequestDetails>(argumentsJson, _serializerOptions);
        
        await context.SendMessageAsync(new InformationRequest(details.Context, details.Entities), cancellationToken: cancellationToken);
    }

    public async ValueTask HandleAsync(InformationResponse message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}