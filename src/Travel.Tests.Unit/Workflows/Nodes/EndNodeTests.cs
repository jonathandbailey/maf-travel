using FluentAssertions;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Extensions;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;
using Travel.Workflows.TravelPlanCriteria.Nodes;
using Travel.Workflows.TravelPlanCriteria.Services;

namespace Travel.Tests.Unit.Workflows.Nodes;

public class EndNodeTests
{
    private static WorkflowRunRequest CreateRequest(Guid threadId)
        => new(new ChatMessage(ChatRole.User, "test"), threadId, new TravelPlanState());

    private static TravelPlanState CreateCompletePlan() => new TravelPlanState(
        origin: "London",
        destination: "Paris",
        startDate: new DateTime(2026, 6, 1),
        endDate: new DateTime(2026, 6, 8),
        numberOfTravelers: 2);

    private static TravelPlanningRunner SetupRunner(Guid threadId, TravelPlanState travelPlan)
    {
        var setupNode = new SetupNode(travelPlan);
        var endNode = new EndNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, endNode);

        var workflow = builder.Build();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        return new TravelPlanningRunner(workflow, CheckpointManager.Default, session);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitTravelPlanningCompleteEvent_WhenPlanIsComplete()
    {
        var threadId = Guid.NewGuid();
        var runner = SetupRunner(threadId, CreateCompletePlan());

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is TravelPlanningCompleteEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldTransitionRunnerToCompleted_WhenPlanIsComplete()
    {
        var threadId = Guid.NewGuid();
        var runner = SetupRunner(threadId, CreateCompletePlan());

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Completed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldIncludeTravelPlan_InCompletedEvent()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        var runner = SetupRunner(threadId, plan);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        var completedEvent = events.OfType<TravelPlanningCompleteEvent>().First();
        completedEvent.TravelPlan.Origin.Should().Be("London");
        completedEvent.TravelPlan.Destination.Should().Be("Paris");
        completedEvent.TravelPlan.NumberOfTravelers.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenOriginIsMissing()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        plan.Origin = null;
        var runner = SetupRunner(threadId, plan);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenDestinationIsMissing()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        plan.Destination = null;
        var runner = SetupRunner(threadId, plan);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenStartDateIsMissing()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        plan.StartDate = null;
        var runner = SetupRunner(threadId, plan);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenEndDateIsMissing()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        plan.EndDate = null;
        var runner = SetupRunner(threadId, plan);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldEmitErrorEvent_WhenNumberOfTravelersIsMissing()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        plan.NumberOfTravelers = null;
        var runner = SetupRunner(threadId, plan);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSetRunnerState_ToFailed_WhenPlanIsIncomplete()
    {
        var threadId = Guid.NewGuid();
        var plan = CreateCompletePlan();
        plan.Origin = null;
        var runner = SetupRunner(threadId, plan);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Failed);
    }

    // Sets threadId + travelPlan in context then forwards TravelPlanCompletedCommand to EndNode
    private class SetupNode(TravelPlanState travelPlan)
        : Executor<WorkflowRunRequest, TravelPlanCompletedCommand>("SetupNode")
    {
        public override async ValueTask<TravelPlanCompletedCommand> HandleAsync(
            WorkflowRunRequest request, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(request.ThreadId, cancellationToken);
            await context.SetTravelPlan(travelPlan, cancellationToken);
            return new TravelPlanCompletedCommand();
        }
    }
}
