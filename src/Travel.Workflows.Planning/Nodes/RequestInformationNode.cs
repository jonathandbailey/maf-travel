using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Planning.Nodes;

public class RequestInformationNode() : ReflectingExecutor<RequestInformationNode>("RequestInformation"), IMessageHandler<FunctionCallContent>
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask HandleAsync(FunctionCallContent functionCallContent, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments["informationRequest"]);
        
        var details = JsonSerializer.Deserialize<InformationRequestDetails>(argumentsJson, _serializerOptions);

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(details);

        var dataContent = new DataContent(jsonBytes, "application/json");

        var message = new ChatMessage(ChatRole.Assistant, [dataContent]);
        
        await context.SendMessageAsync(new InformationRequest(message), cancellationToken: cancellationToken);
    }
}