using FluentAssertions;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Common.Exceptions;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;
using Travel.Workflows.TravelPlanCriteria.Services;

namespace Travel.Tests.Unit.Workflows;

public class TravelPlannerRunnerTests
{
    private static WorkflowRunRequest CreateRequest(Guid threadId) =>
        new(new ChatMessage(ChatRole.User, "Test message"), threadId, new TravelPlanState());

    private static (TravelPlanningRunner runner, WorkflowRunRequest request) SetupRunner(
        Executor startNode,
        WorkflowState state = WorkflowState.Created,
        CheckpointInfo? checkpoint = null)
    {
        var workflow = new WorkflowBuilder(startNode).Build();
        var threadId = Guid.NewGuid();
        var session = new WorkflowSession(threadId, state, checkpoint);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);
        return (runner, CreateRequest(threadId));
    }

    [Fact]
    [Trait("Category", "Unit")]

    public void State_ShouldBe_Created_OnInitialization()
    {
        var (runner, _) = SetupRunner(new CompletingNode());

        runner.Session.State.Should().Be(WorkflowState.Created);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public void LastCheckpoint_ShouldBe_Null_WhenSessionHasNoCheckpoint()
    {
        var (runner, _) = SetupRunner(new CompletingNode());

        runner.Session.LastCheckpoint.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldYield_TravelPlanningCompleteEvent()
    {
        var (runner, request) = SetupRunner(new CompletingNode());
        var events = new List<WorkflowEvent>();

        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().ContainSingle(e => e is TravelPlanningCompleteEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldSetState_Completed_AfterTravelPlanningCompleteEvent()
    {
        var (runner, request) = SetupRunner(new CompletingNode());

        await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Completed);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldYield_TravelPlanStatusUpdateEvent()
    {
        var (runner, request) = SetupRunner(new StatusUpdateNode());
        var events = new List<WorkflowEvent>();

        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().ContainSingle(e => e is TravelPlanStatusUpdateEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldYield_TravelPlanUpdateEvent()
    {
        var (runner, request) = SetupRunner(new PlanUpdateNode());
        var events = new List<WorkflowEvent>();

        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().ContainSingle(e => e is TravelPlanUpdateEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldYield_RequestInfoEvent_AndSetState_Suspended_WhenExecuting()
    {
        var suspendNode = new SuspendNode();
        var requestInfoPort = RequestPort.Create<InformationRequest, InformationResponse>("information");

        var builder = new WorkflowBuilder(suspendNode);
        builder.AddEdge(suspendNode, requestInfoPort);
        builder.AddEdge(requestInfoPort, suspendNode);
        var workflow = builder.Build();

        var threadId = Guid.NewGuid();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);
        var request = CreateRequest(threadId);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().ContainSingle(e => e is RequestInfoEvent);
        runner.Session.State.Should().Be(WorkflowState.Suspended);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldThrow_WorkflowException_WhenState_IsFailed()
    {
        var (runner, request) = SetupRunner(new CompletingNode(), WorkflowState.Failed);

        var act = async () =>
        {
            await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }
        };

        await act.Should().ThrowAsync<WorkflowException>();
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldThrow_WorkflowException_WhenState_IsExecuting()
    {
        var (runner, request) = SetupRunner(new CompletingNode(), WorkflowState.Executing);

        var act = async () =>
        {
            await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }
        };

        await act.Should().ThrowAsync<WorkflowException>();
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldRestartWorkflow_WhenState_IsCompleted()
    {
        var (runner, request) = SetupRunner(new CompletingNode(), WorkflowState.Completed);
        var events = new List<WorkflowEvent>();

        await foreach (var evt in runner.WatchStreamAsync(request, CancellationToken.None))
            events.Add(evt);

        events.Should().ContainSingle(e => e is TravelPlanningCompleteEvent);
        runner.Session.State.Should().Be(WorkflowState.Completed);
    }

    [Fact]
    [Trait("Category", "Unit")]

    public async Task WatchStreamAsync_ShouldThrow_WorkflowException_WhenSuspended_WithNullCheckpoint()
    {
        var (runner, request) = SetupRunner(new CompletingNode(), WorkflowState.Suspended, checkpoint: null);

        var act = async () =>
        {
            await foreach (var _ in runner.WatchStreamAsync(request, CancellationToken.None)) { }
        };

        await act.Should().ThrowAsync<WorkflowException>();
    }

    private class CompletingNode() : Executor<WorkflowRunRequest>("CompletingNode")
    {
        public override async ValueTask HandleAsync(WorkflowRunRequest message, IWorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            await context.AddEventAsync(new TravelPlanningCompleteEvent(new TravelPlanState()), cancellationToken);
        }
    }

    private class StatusUpdateNode() : Executor<WorkflowRunRequest>("StatusUpdateNode")
    {
        public override async ValueTask HandleAsync(WorkflowRunRequest message, IWorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            await context.AddEventAsync(new TravelPlanStatusUpdateEvent("Processing"), cancellationToken);
        }
    }

    private class PlanUpdateNode() : Executor<WorkflowRunRequest>("PlanUpdateNode")
    {
        public override async ValueTask HandleAsync(WorkflowRunRequest message, IWorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            await context.AddEventAsync(new TravelPlanUpdateEvent(new TravelPlanState()), cancellationToken);
        }
    }

    private class SuspendNode() : Executor<WorkflowRunRequest, InformationRequest>("SuspendNode")
    {
        public override async ValueTask<InformationRequest> HandleAsync(WorkflowRunRequest message, IWorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            return new InformationRequest("Please provide more details", new List<string>());
        }
    }
}
