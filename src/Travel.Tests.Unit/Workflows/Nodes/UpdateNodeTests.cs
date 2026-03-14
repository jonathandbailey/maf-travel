using FluentAssertions;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Nodes;
using Travel.Workflows.Services;

namespace Travel.Tests.Unit.Workflows.Nodes;

public class UpdateNodeTests
{
    private static (TravelPlanningRunner runner, CapturingNode capturingNode) SetupRunner(
        TravelPlanState? initialPlan = null,
        TravelPlanData? patchData = null,
        Guid? sessionThreadId = null)
    {
        var threadId = sessionThreadId ?? Guid.NewGuid();
        var resolvedInitial = initialPlan ?? new TravelPlanState();
        var resolvedPatch = patchData ?? new TravelPlanData();

        var setupNode = new SetupNode(threadId, resolvedInitial, resolvedPatch);
        var updateNode = new UpdateNode();
        var capturingNode = new CapturingNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, updateNode);
        builder.AddEdge(updateNode, capturingNode);
        var workflow = builder.Build();

        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        return (runner, capturingNode);
    }

    private static WorkflowRunRequest CreateRequest(Guid? threadId = null)
    {
        var id = threadId ?? Guid.NewGuid();
        return new WorkflowRunRequest(new ChatMessage(ChatRole.User, "test"), id, new TravelPlanState());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldApplyPatch_ToExistingTravelPlan()
    {
        var threadId = Guid.NewGuid();
        var initial = new TravelPlanState { Origin = "London" };
        var patch = new TravelPlanData(Destination: "Paris");
        var (runner, capturingNode) = SetupRunner(initial, patch, threadId);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedTravelPlan.Should().NotBeNull();
        capturingNode.CapturedTravelPlan!.Origin.Should().Be("London");
        capturingNode.CapturedTravelPlan.Destination.Should().Be("Paris");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldNotOverwriteExistingField_WhenPatchFieldIsNull()
    {
        var threadId = Guid.NewGuid();
        var initial = new TravelPlanState { Origin = "London", Destination = "Rome" };
        var patch = new TravelPlanData(Destination: "Paris"); // Origin is null in patch
        var (runner, capturingNode) = SetupRunner(initial, patch, threadId);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedTravelPlan!.Origin.Should().Be("London");
        capturingNode.CapturedTravelPlan.Destination.Should().Be("Paris");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldPersistUpdatedPlan_InContext()
    {
        var threadId = Guid.NewGuid();
        var initial = new TravelPlanState();
        var patch = new TravelPlanData(Origin: "Berlin", Destination: "Tokyo", NumberOfTravelers: 3);
        var (runner, capturingNode) = SetupRunner(initial, patch, threadId);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedTravelPlan!.Origin.Should().Be("Berlin");
        capturingNode.CapturedTravelPlan.Destination.Should().Be("Tokyo");
        capturingNode.CapturedTravelPlan.NumberOfTravelers.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitTravelPlanUpdateEvent()
    {
        var threadId = Guid.NewGuid();
        var (runner, _) = SetupRunner(sessionThreadId: threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is TravelPlanUpdateEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitTravelPlanUpdateEvent_WithUpdatedPlan()
    {
        var threadId = Guid.NewGuid();
        var patch = new TravelPlanData(Origin: "Sydney", Destination: "Auckland");
        var (runner, _) = SetupRunner(new TravelPlanState(), patch, threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        var updateEvent = events.OfType<TravelPlanUpdateEvent>().First();
        updateEvent.TravelPlanState.Origin.Should().Be("Sydney");
        updateEvent.TravelPlanState.Destination.Should().Be("Auckland");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenTravelPlanIsNull()
    {
        var threadId = Guid.NewGuid();
        var setupNode = new NullPlanSetupNode(threadId);
        var updateNode = new UpdateNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, updateNode);
        var workflow = builder.Build();

        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenTravelPlanIsNull()
    {
        var threadId = Guid.NewGuid();
        var setupNode = new NullPlanSetupNode(threadId);
        var updateNode = new UpdateNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, updateNode);
        var workflow = builder.Build();

        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    // Sets up context with initial plan and thread ID, then sends TravelPlanUpdateCommand with the patch
    private class SetupNode(Guid threadId, TravelPlanState initialPlan, TravelPlanData patchData)
        : Executor<WorkflowRunRequest, TravelPlanUpdateCommand>("SetupNode")
    {
        public override async ValueTask<TravelPlanUpdateCommand> HandleAsync(
            WorkflowRunRequest message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(threadId, cancellationToken);
            await context.SetTravelPlan(initialPlan, cancellationToken);
            return new TravelPlanUpdateCommand(patchData);
        }
    }

    // Sends a TravelPlanUpdateCommand with a null Data to trigger validation failure
    private class NullPlanSetupNode(Guid threadId)
        : Executor<WorkflowRunRequest, TravelPlanUpdateCommand>("NullPlanSetupNode")
    {
        public override async ValueTask<TravelPlanUpdateCommand> HandleAsync(
            WorkflowRunRequest message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(threadId, cancellationToken);
            await context.SetTravelPlan(new TravelPlanState(), cancellationToken);
            return new TravelPlanUpdateCommand(null!);
        }
    }

    private class CapturingNode() : Executor<TravelPlanContextUpdated>("CapturingNode")
    {
        public TravelPlanState? CapturedTravelPlan { get; private set; }

        public override async ValueTask HandleAsync(
            TravelPlanContextUpdated message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            CapturedTravelPlan = await context.GetTravelPlan(cancellationToken);
            await context.AddEventAsync(new TravelPlanningCompleteEvent(CapturedTravelPlan), cancellationToken);
        }
    }
}
