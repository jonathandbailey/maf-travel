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

namespace Travel.Tests.Unit.Workflows;

public class ExtractionNodeTests
{
    private static TravelWorkflowRequest CreateRequest(Guid threadId, string message = "I want to travel to Paris")
        => new(new ChatMessage(ChatRole.User, message), threadId, new TravelPlanDto());

    private static IChatClient CreateMockChatClient(TravelPlanDto travelPlan)
    {
        var mockChatClient = new Mock<IChatClient>();

        var travelPlanElement = System.Text.Json.JsonSerializer.SerializeToElement(travelPlan);
        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: ExtractingTools.UpdateTravelPlanToolName,
            arguments: new Dictionary<string, object?>
            {
                [WorkflowConstants.ExtractingNodeUpdatePlanFunctionName] = travelPlanElement
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

    private static IChatClient CreateChatClientWithNoToolCall()
    {
        var mockChatClient = new Mock<IChatClient>();

        var responseMessage = new ChatMessage(ChatRole.Assistant, "I cannot help with that.");

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    private static async Task<ExtractionNode> CreateExtractionNode(IChatClient chatClient)
    {
        var agentFactory = new CustomPromptAgentFactory(chatClient, tools: ExtractingTools.GetDeclarationOnlyTools());
        var agent = await agentFactory.CreateFromYamlAsync(StubAgentTemplate.Yaml);
        return new ExtractionNode(agent);
    }

    private static (TravelPlanningRunner runner, CapturingNode capturingNode) SetupRunner(
        ExtractionNode extractionNode,
        Func<TravelWorkflowRequest, TravelPlanExtractCommand>? commandFactory = null)
    {
        var factory = commandFactory ?? (req => new TravelPlanExtractCommand(req.Message));
        var setupNode = new SetupNode(factory);
        var capturingNode = new CapturingNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, extractionNode);
        builder.AddEdge(extractionNode, capturingNode);
        var workflow = builder.Build();

        var threadId = Guid.NewGuid();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        return (runner, capturingNode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendTravelPlanUpdateCommand_WithExtractedPlan()
    {
        var threadId = Guid.NewGuid();
        var expectedPlan = new TravelPlanDto { Origin = "London", Destination = "Paris" };
        var extractionNode = await CreateExtractionNode(CreateMockChatClient(expectedPlan));
        var (runner, capturingNode) = SetupRunner(extractionNode);
        var request = CreateRequest(threadId);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        capturingNode.CapturedCommand.Should().NotBeNull();
        capturingNode.CapturedCommand!.TravelPlan.Origin.Should().Be("London");
        capturingNode.CapturedCommand.TravelPlan.Destination.Should().Be("Paris");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitTravelPlanStatusUpdateEvent_AfterAgentResponds()
    {
        var threadId = Guid.NewGuid();
        var extractionNode = await CreateExtractionNode(CreateMockChatClient(new TravelPlanDto()));
        var (runner, _) = SetupRunner(extractionNode);
        var request = CreateRequest(threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is TravelPlanStatusUpdateEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitStatusUpdate_WithExtractionNodeSource()
    {
        var threadId = Guid.NewGuid();
        var extractionNode = await CreateExtractionNode(CreateMockChatClient(new TravelPlanDto()));
        var (runner, _) = SetupRunner(extractionNode);
        var request = CreateRequest(threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        var statusEvent = events.OfType<TravelPlanStatusUpdateEvent>().First();
        statusEvent.Source.Should().Be(NodeNames.ExtractionNodeName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenMessageTextIsEmpty()
    {
        var threadId = Guid.NewGuid();
        var extractionNode = await CreateExtractionNode(CreateMockChatClient(new TravelPlanDto()));
        var (runner, _) = SetupRunner(
            extractionNode,
            commandFactory: _ => new TravelPlanExtractCommand(new ChatMessage(ChatRole.User, "")));
        var request = CreateRequest(threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenAgentThrows()
    {
        var threadId = Guid.NewGuid();
        var extractionNode = await CreateExtractionNode(CreateThrowingChatClient());
        var (runner, _) = SetupRunner(extractionNode);
        var request = CreateRequest(threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenAgentReturnsNoToolCall()
    {
        var threadId = Guid.NewGuid();
        var extractionNode = await CreateExtractionNode(CreateChatClientWithNoToolCall());
        var (runner, _) = SetupRunner(extractionNode);
        var request = CreateRequest(threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenAgentThrows()
    {
        var threadId = Guid.NewGuid();
        var extractionNode = await CreateExtractionNode(CreateThrowingChatClient());
        var (runner, _) = SetupRunner(extractionNode);
        var request = CreateRequest(threadId);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    private class SetupNode(Func<TravelWorkflowRequest, TravelPlanExtractCommand> commandFactory)
        : Executor<TravelWorkflowRequest, TravelPlanExtractCommand>("SetupNode")
    {
        public override async ValueTask<TravelPlanExtractCommand> HandleAsync(
            TravelWorkflowRequest message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(message.ThreadId, cancellationToken);
            await context.SetTravelPlan(message.TravelPlan, cancellationToken);
            return commandFactory(message);
        }
    }

    private class CapturingNode() : Executor<TravelPlanUpdateCommand>("CapturingNode")
    {
        public TravelPlanUpdateCommand? CapturedCommand { get; private set; }

        public override async ValueTask HandleAsync(
            TravelPlanUpdateCommand command, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            CapturedCommand = command;
            await context.AddEventAsync(new TravelPlanningCompleteEvent(command.TravelPlan), cancellationToken);
        }
    }
}
