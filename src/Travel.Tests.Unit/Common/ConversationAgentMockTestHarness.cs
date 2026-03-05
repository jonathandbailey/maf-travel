using System.Runtime.CompilerServices;
using System.Text.Json;
using Agents.Extensions;
using Agents.Services;
using Agents.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Experience.Application.Agents;
using Travel.Experience.Application.Dto;
using Travel.Tests.Unit.TestData;
using Travel.Workflows.Interfaces;

namespace Travel.Tests.Unit.Common;

public class ConversationAgentMockTestHarness
{
    public List<AgentResponseUpdate> Updates { get; } = [];

    public async Task RunAsync(ConversationAgentRun run, Guid threadId)
    {
        var (conversationAgent, session) = await BuildAsync(run);

        var options = new ChatClientAgentRunOptions();
        options.AddAgUiThreadId(threadId.ToString());

        var updates = await conversationAgent
            .RunStreamingAsync(run.Message, session, options: options)
            .ToListAsync();

        Updates.AddRange(updates);
    }

    public IEnumerable<TravelPlanState> GetTravelPlanSnapshots()
    {
        foreach (var update in Updates)
        {
            foreach (var content in update.Contents.OfType<DataContent>())
            {
                var snapshot = JsonSerializer.Deserialize<SnapShot<TravelPlanState>>(
                    content.Data.ToArray(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (snapshot?.Type == "TravelPlanUpdate" && snapshot.Payload is not null)
                    yield return snapshot.Payload;
            }
        }
    }

    private static async Task<(ConversationAgent agent, AgentSession session)> BuildAsync(ConversationAgentRun run)
    {
        var mockWorkflowFactory = new Mock<IWorkflowFactory>();
        var declarationOnlyTools = new TravelWorkflowToolHandler(() => mockWorkflowFactory.Object).GetDeclarationOnlyTools();

        var mockChatClient = new Mock<IChatClient>();

        var arguments = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(run.ConversationAgentMeta.ArgumentsKey) && run.ConversationAgentMeta.Arguments is not null)
            arguments[run.ConversationAgentMeta.ArgumentsKey] = JsonSerializer.SerializeToElement(run.ConversationAgentMeta.Arguments);

        var toolCall = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: run.ConversationAgentMeta.Name,
            arguments: arguments);

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, [toolCall])));

        mockChatClient
            .Setup(c => c.GetStreamingResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(EmptyStreamAsync());


        var agentFactory = new CustomPromptAgentFactory(mockChatClient.Object, tools: declarationOnlyTools);
        var innerAgent = await agentFactory.CreateFromYamlAsync(StubAgentTemplate.Yaml);

        var mockHandler = new Mock<IToolHandler>();
        mockHandler.Setup(h => h.ToolName).Returns(TravelWorkflowToolHandler.RequestInformationToolName);
        mockHandler.Setup(h => h.GetDeclarationOnlyTools()).Returns(declarationOnlyTools);
        mockHandler
            .Setup(h => h.ExecuteAsync(
                It.IsAny<FunctionCallContent>(),
                It.IsAny<ToolHandlerContext>(),
                It.IsAny<CancellationToken>()))
            .Returns((FunctionCallContent call, ToolHandlerContext _, CancellationToken ct) =>
                BuildUpdatesStream(call, run.ToolHandlerResults, ct));

        var registry = new ToolRegistry([mockHandler.Object]);
        var conversationAgent = new ConversationAgent(innerAgent, registry);
        var session = await conversationAgent.CreateSessionAsync();

        return (conversationAgent, session);
    }

    private static async IAsyncEnumerable<ToolHandlerUpdate> BuildUpdatesStream(
        FunctionCallContent call,
        List<ToolHandlerResultMeta> results,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return result.Type switch
            {
                "ToolStatusUpdate" =>
                    new ToolStatusUpdate(result.Message ?? string.Empty, result.Thought),
                "ToolStateSnapshotUpdate" =>
                    new ToolStateSnapshotUpdate(result.SnapshotType!, result.Data!.Value),
                "ToolResultUpdate" =>
                    new ToolResultUpdate(new FunctionResultContent(call.CallId, result.Data?.ToString() ?? string.Empty)),
                _ => throw new InvalidOperationException($"Unknown ToolHandlerUpdate type: {result.Type}")
            };
        }
    }

    private static async IAsyncEnumerable<ChatResponseUpdate> EmptyStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield break;
    }
}
