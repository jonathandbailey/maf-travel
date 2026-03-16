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

public class InformationRequestNodeTests
{
    private static WorkflowRunRequest CreateRequest(Guid threadId)
        => new(new ChatMessage(ChatRole.User, "test"), threadId, new TravelPlanState());

    private static TravelPlanningRunner SetupRunner(Guid threadId, RequestInformationCommand command)
    {
        var setupNode = new SetupNode(command);
        var informationRequestNode = new InformationRequestNode();
        var requestPort = RequestPort.Create<InformationRequest, InformationResponse>("information");
        var placeholderNode = new PlaceholderNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, informationRequestNode);
        builder.AddEdge(informationRequestNode, requestPort);
        builder.AddEdge(requestPort, placeholderNode);

        var workflow = builder.Build();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        return new TravelPlanningRunner(workflow, CheckpointManager.Default, session);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSuspendWorkflow_WhenReceivingRequestInformationCommand()
    {
        var threadId = Guid.NewGuid();
        var dto = new RequestInformationDto("What is your origin?", "Need origin city", ["origin"]);
        var runner = SetupRunner(threadId, new RequestInformationCommand(dto));

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().Be(WorkflowState.Suspended);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldYieldRequestInfoEvent_WhenReceivingRequestInformationCommand()
    {
        var threadId = Guid.NewGuid();
        var dto = new RequestInformationDto("What is your origin?", "Need origin city", ["origin"]);
        var runner = SetupRunner(threadId, new RequestInformationCommand(dto));

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().Contain(e => e is RequestInfoEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldNotReachCompleted_WhenSuspended()
    {
        var threadId = Guid.NewGuid();
        var dto = new RequestInformationDto("What city are you departing from?", "Thinking...", ["origin"]);
        var runner = SetupRunner(threadId, new RequestInformationCommand(dto));

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        runner.Session.State.Should().NotBe(WorkflowState.Completed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldNotReachFailed_WhenGivenValidCommand()
    {
        var threadId = Guid.NewGuid();
        var dto = new RequestInformationDto("What city are you departing from?", "Thinking...", ["origin"]);
        var runner = SetupRunner(threadId, new RequestInformationCommand(dto));

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().NotContain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
        runner.Session.State.Should().NotBe(WorkflowState.Failed);
    }

    // Sets threadId then forwards the RequestInformationCommand to InformationRequestNode
    private class SetupNode(RequestInformationCommand command)
        : Executor<WorkflowRunRequest, RequestInformationCommand>("SetupNode")
    {
        public override async ValueTask<RequestInformationCommand> HandleAsync(
            WorkflowRunRequest request, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(request.ThreadId, cancellationToken);
            return command;
        }
    }

    // Never invoked during a single-pass test (workflow suspends before port response is processed)
    private class PlaceholderNode() : Executor<InformationResponse>("PlaceholderNode")
    {
        public override async ValueTask HandleAsync(
            InformationResponse response, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.AddEventAsync(new TravelPlanningCompleteEvent(new TravelPlanState()), cancellationToken);
        }
    }
}
