using Agents.Services;
using FluentAssertions;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
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

public class PlannerNodeTests
{
    private static WorkflowRunRequest CreateRequest(Guid threadId)
        => new(new ChatMessage(ChatRole.User, "test"), threadId, new TravelPlanState());

    private static IChatClient CreateMockChatClientWithToolCall(string toolName)
    {
        var mockChatClient = new Mock<IChatClient>();

        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: toolName,
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

    private static IChatClient CreateMockChatClientWithTextResponse(string text = "Thinking about the plan...")
    {
        var mockChatClient = new Mock<IChatClient>();

        var responseMessage = new ChatMessage(ChatRole.Assistant, text);

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    private static IChatClient CreateThrowingChatClient()
    {
        var mockChatClient = new Mock<IChatClient>();

        mockChatClient
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated agent failure"));

        return mockChatClient.Object;
    }

    private static async Task<PlannerNode> CreatePlannerNode(IChatClient chatClient)
    {
        var agentFactory = new CustomPromptAgentFactory(chatClient, tools: new PlanningToolsHandler().GetDeclarationOnlyTools());
        var agent = await agentFactory.CreateFromYamlAsync(StubAgentTemplate.Yaml);
        return new PlannerNode(agent);
    }

    private static (TravelPlanningRunner runner, CapturingNode capturingNode) SetupRunner(
        PlannerNode plannerNode,
        TravelPlanState? travelPlan = null)
    {
        var resolvedPlan = travelPlan ?? new TravelPlanState { Origin = "London", Destination = "Paris" };
        var setupNode = new SetupNode(resolvedPlan);
        var capturingNode = new CapturingNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, plannerNode);
        builder.AddEdge(plannerNode, capturingNode);
        var workflow = builder.Build();

        var threadId = Guid.NewGuid();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        return (runner, capturingNode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendAgentResponse_ToNextNode()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateMockChatClientWithToolCall(PlanningToolsHandler.PlanningCompleteToolName));
        var (runner, capturingNode) = SetupRunner(plannerNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedResponse.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendAgentResponse_WithPlanningCompleteToolCall()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateMockChatClientWithToolCall(PlanningToolsHandler.PlanningCompleteToolName));
        var (runner, capturingNode) = SetupRunner(plannerNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        var toolCalls = capturingNode.CapturedResponse!.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();
        toolCalls.Should().Contain(fc => fc.Name == PlanningToolsHandler.PlanningCompleteToolName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendAgentResponse_WithRequestInformationToolCall()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateMockChatClientWithToolCall(PlanningToolsHandler.RequestInformationToolName));
        var (runner, capturingNode) = SetupRunner(plannerNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        var toolCalls = capturingNode.CapturedResponse!.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();
        toolCalls.Should().Contain(fc => fc.Name == PlanningToolsHandler.RequestInformationToolName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitTravelPlanStatusUpdateEvent()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateMockChatClientWithToolCall(PlanningToolsHandler.PlanningCompleteToolName));
        var (runner, _) = SetupRunner(plannerNode);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is TravelPlanStatusUpdateEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitStatusUpdate_WithPlannerNodeSource()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateMockChatClientWithToolCall(PlanningToolsHandler.PlanningCompleteToolName));
        var (runner, _) = SetupRunner(plannerNode);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        var statusEvent = events.OfType<TravelPlanStatusUpdateEvent>().First();
        statusEvent.Source.Should().Be(NodeNames.PlannerNode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenAgentThrows()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateThrowingChatClient());
        var (runner, _) = SetupRunner(plannerNode);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenAgentThrows()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateThrowingChatClient());
        var (runner, _) = SetupRunner(plannerNode);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenTravelPlanIsNotInContext()
    {
        var threadId = Guid.NewGuid();
        var plannerNode = await CreatePlannerNode(CreateMockChatClientWithToolCall(PlanningToolsHandler.PlanningCompleteToolName));
        var setupNode = new NoPlanSetupNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, plannerNode);
        var workflow = builder.Build();

        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    // Sets context (threadId + travelPlan) then sends TravelPlanContextUpdated to PlannerNode
    private class SetupNode(TravelPlanState travelPlan) : Executor<WorkflowRunRequest, TravelPlanContextUpdated>("SetupNode")
    {
        public override async ValueTask<TravelPlanContextUpdated> HandleAsync(
            WorkflowRunRequest message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(message.ThreadId, cancellationToken);
            await context.SetTravelPlan(travelPlan, cancellationToken);
            return new TravelPlanContextUpdated();
        }
    }

    // Sets threadId but does NOT set TravelPlan — triggers missing plan error
    private class NoPlanSetupNode() : Executor<WorkflowRunRequest, TravelPlanContextUpdated>("NoPlanSetupNode")
    {
        public override async ValueTask<TravelPlanContextUpdated> HandleAsync(
            WorkflowRunRequest message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(message.ThreadId, cancellationToken);
            return new TravelPlanContextUpdated();
        }
    }

    private class CapturingNode() : Executor<AgentResponse>("CapturingNode")
    {
        public AgentResponse? CapturedResponse { get; private set; }

        public override async ValueTask HandleAsync(
            AgentResponse response, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            CapturedResponse = response;
            await context.AddEventAsync(new TravelPlanningCompleteEvent(new TravelPlanState()), cancellationToken);
        }
    }
}
