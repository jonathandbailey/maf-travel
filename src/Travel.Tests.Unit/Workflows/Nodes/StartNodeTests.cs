using FluentAssertions;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;
using Travel.Workflows.TravelPlanCriteria.Nodes;
using Travel.Workflows.TravelPlanCriteria.Services;

namespace Travel.Tests.Unit.Workflows.Nodes;

public class StartNodeTests
{
    private static WorkflowRunRequest CreateRequest(
        Guid? threadId = null,
        string message = "I want to travel to Paris",
        ChatRole? role = null,
        TravelPlanState? travelPlan = null)
    {
        var id = threadId ?? Guid.NewGuid();
        var chatMessage = new ChatMessage(role ?? ChatRole.User, message);
        return new WorkflowRunRequest(chatMessage, id, travelPlan ?? new TravelPlanState());
    }

    private static (TravelPlanningRunner runner, CapturingNode capturingNode) SetupRunner(Guid? sessionThreadId = null)
    {
        var startNode = new StartNode();
        var capturingNode = new CapturingNode();

        var builder = new WorkflowBuilder(startNode);
        builder.AddEdge(startNode, capturingNode);
        var workflow = builder.Build();

        var threadId = sessionThreadId ?? Guid.NewGuid();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        return (runner, capturingNode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendTravelPlanExtractCommand_WithRequestMessage()
    {
        var threadId = Guid.NewGuid();
        var (runner, capturingNode) = SetupRunner(threadId);
        var message = "I want to go to Paris next month";
        var request = CreateRequest(threadId, message);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        capturingNode.CapturedCommand.Should().NotBeNull();
        capturingNode.CapturedCommand!.Message.Text.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldPreserveMessageRole_InExtractCommand()
    {
        var threadId = Guid.NewGuid();
        var (runner, capturingNode) = SetupRunner(threadId);
        var request = CreateRequest(threadId);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        capturingNode.CapturedCommand!.Message.Role.Should().Be(ChatRole.User);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetTravelPlan_InContext()
    {
        var threadId = Guid.NewGuid();
        var travelPlan = new TravelPlanState { Origin = "London", Destination = "Paris" };
        var (runner, capturingNode) = SetupRunner(threadId);
        var request = CreateRequest(threadId, travelPlan: travelPlan);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        capturingNode.CapturedTravelPlan.Should().NotBeNull();
        capturingNode.CapturedTravelPlan!.Origin.Should().Be("London");
        capturingNode.CapturedTravelPlan.Destination.Should().Be("Paris");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetThreadId_InContext()
    {
        var threadId = Guid.NewGuid();
        var (runner, capturingNode) = SetupRunner(threadId);
        var request = CreateRequest(threadId);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        capturingNode.CapturedThreadId.Should().Be(threadId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenThreadIdIsEmpty()
    {
        var (runner, _) = SetupRunner();
        var request = CreateRequest(threadId: Guid.Empty);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenThreadIdIsEmpty()
    {
        var (runner, _) = SetupRunner();
        var request = CreateRequest(threadId: Guid.Empty);

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenMessageRoleIsNotUser()
    {
        var threadId = Guid.NewGuid();
        var (runner, _) = SetupRunner(threadId);
        var request = CreateRequest(threadId, role: ChatRole.Assistant);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldThrowWorkflowException_WhenMessageTextIsEmpty()
    {
        var threadId = Guid.NewGuid();
        var (runner, _) = SetupRunner(threadId);
        var request = CreateRequest(threadId, message: "");

        var act = async () =>
        {
            await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }
        };

        await act.Should().ThrowAsync<WorkflowException>();
    }

    private class CapturingNode() : Executor<TravelPlanExtractCommand>("CapturingNode")
    {
        public TravelPlanExtractCommand? CapturedCommand { get; private set; }
        public TravelPlanState? CapturedTravelPlan { get; private set; }
        public Guid CapturedThreadId { get; private set; }

        public override async ValueTask HandleAsync(TravelPlanExtractCommand command, IWorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            CapturedCommand = command;
            CapturedTravelPlan = await context.GetTravelPlan(cancellationToken);
            CapturedThreadId = await context.GetThreadId(cancellationToken);
            await context.AddEventAsync(new TravelPlanningCompleteEvent(CapturedTravelPlan), cancellationToken);
        }
    }
}
