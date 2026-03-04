using Agents.Services;
using FluentAssertions;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using System.Text.Json;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Unit.Common;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Nodes;
using Travel.Workflows.Services;

namespace Travel.Tests.Unit.Workflows.Nodes;

public class ExecutionNodeTests
{
    private static TravelWorkflowRequest CreateRequest(Guid threadId)
        => new(new ChatMessage(ChatRole.User, "test"), threadId, new TravelPlanDto());

    private static IChatClient CreateMockChatClientWithPlanningComplete()
    {
        var mockChatClient = new Mock<IChatClient>();

        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: PlanningToolsHandler.PlanningCompleteToolName,
            arguments: null);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    private static IChatClient CreateMockChatClientWithRequestInformation(RequestInformationDto dto)
    {
        var mockChatClient = new Mock<IChatClient>();

        var dtoElement = JsonSerializer.SerializeToElement(dto, Json.FunctionCallSerializerOptions);
        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: PlanningToolsHandler.RequestInformationToolName,
            arguments: new Dictionary<string, object?>
            {
                [WorkflowConstants.InformationRequestFunctionArgumentName] = dtoElement
            });

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    private static IChatClient CreateMockChatClientWithNoToolCall()
    {
        var mockChatClient = new Mock<IChatClient>();

        var responseMessage = new ChatMessage(ChatRole.Assistant, "I'm thinking...");

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    private static IChatClient CreateMockChatClientWithUnknownToolCall()
    {
        var mockChatClient = new Mock<IChatClient>();

        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: "unknown_tool",
            arguments: null);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    private static async Task<SetupNode> CreateSetupNode(IChatClient chatClient)
    {
        var agentFactory = new CustomPromptAgentFactory(chatClient, tools: new PlanningToolsHandler().GetDeclarationOnlyTools());
        var agent = await agentFactory.CreateFromYamlAsync(StubAgentTemplate.Yaml);
        return new SetupNode(agent);
    }

    private static (TravelPlanningRunner runner, RequestInfoCapturingNode requestInfoNode, CompletedCapturingNode completedNode)
        SetupRunner(SetupNode setupNode)
    {
        var executionNode = new ExecutionNode();
        var requestInfoNode = new RequestInfoCapturingNode();
        var completedNode = new CompletedCapturingNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, executionNode);
        builder.AddEdge(executionNode, requestInfoNode);
        builder.AddEdge(executionNode, completedNode);

        var workflow = builder.Build();

        var threadId = Guid.NewGuid();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        return (runner, requestInfoNode, completedNode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendTravelPlanCompletedCommand_WhenPlanningCompleteToolCall()
    {
        var threadId = Guid.NewGuid();
        var setupNode = await CreateSetupNode(CreateMockChatClientWithPlanningComplete());
        var (runner, _, completedNode) = SetupRunner(setupNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        completedNode.WasInvoked.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendRequestInformationCommand_WhenRequestInformationToolCall()
    {
        var threadId = Guid.NewGuid();
        var dto = new RequestInformationDto("What is your origin?", "Missing origin", ["origin"]);
        var setupNode = await CreateSetupNode(CreateMockChatClientWithRequestInformation(dto));
        var (runner, requestInfoNode, _) = SetupRunner(setupNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        requestInfoNode.CapturedCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldPassRequestInformationDetails_InRequestInformationCommand()
    {
        var threadId = Guid.NewGuid();
        var dto = new RequestInformationDto("What is your origin?", "Missing origin", ["origin", "destination"]);
        var setupNode = await CreateSetupNode(CreateMockChatClientWithRequestInformation(dto));
        var (runner, requestInfoNode, _) = SetupRunner(setupNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        requestInfoNode.CapturedCommand!.Details.Message.Should().Be("What is your origin?");
        requestInfoNode.CapturedCommand!.Details.RequiredInputs.Should().BeEquivalentTo(["origin", "destination"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenNoToolCallInAgentResponse()
    {
        var threadId = Guid.NewGuid();
        var setupNode = await CreateSetupNode(CreateMockChatClientWithNoToolCall());
        var (runner, _, _) = SetupRunner(setupNode);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenNoToolCallInAgentResponse()
    {
        var threadId = Guid.NewGuid();
        var setupNode = await CreateSetupNode(CreateMockChatClientWithNoToolCall());
        var (runner, _, _) = SetupRunner(setupNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenUnknownToolCallName()
    {
        var threadId = Guid.NewGuid();
        var setupNode = await CreateSetupNode(CreateMockChatClientWithUnknownToolCall());
        var (runner, _, _) = SetupRunner(setupNode);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenUnknownToolCallName()
    {
        var threadId = Guid.NewGuid();
        var setupNode = await CreateSetupNode(CreateMockChatClientWithUnknownToolCall());
        var (runner, _, _) = SetupRunner(setupNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    // Sets threadId, runs the agent, and forwards the AgentResponse to ExecutionNode
    private class SetupNode(AIAgent agent) : Executor<TravelWorkflowRequest, AgentResponse>("SetupNode")
    {
        public override async ValueTask<AgentResponse> HandleAsync(
            TravelWorkflowRequest message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(message.ThreadId, cancellationToken);
            return await agent.RunAsync("test", cancellationToken: cancellationToken);
        }
    }

    private class RequestInfoCapturingNode() : Executor<RequestInformationCommand>("RequestInfoCapturingNode")
    {
        public RequestInformationCommand? CapturedCommand { get; private set; }

        public override async ValueTask HandleAsync(
            RequestInformationCommand command, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            CapturedCommand = command;
            await context.AddEventAsync(new TravelPlanningCompleteEvent(new TravelPlanDto()), cancellationToken);
        }
    }

    private class CompletedCapturingNode() : Executor<TravelPlanCompletedCommand>("CompletedCapturingNode")
    {
        public bool WasInvoked { get; private set; }

        public override async ValueTask HandleAsync(
            TravelPlanCompletedCommand command, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            WasInvoked = true;
            await context.AddEventAsync(new TravelPlanningCompleteEvent(new TravelPlanDto()), cancellationToken);
        }
    }
}
