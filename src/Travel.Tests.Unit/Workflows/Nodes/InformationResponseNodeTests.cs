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

public class InformationResponseNodeTests
{
    private static TravelWorkflowRequest CreateRequest(Guid threadId)
        => new(new ChatMessage(ChatRole.User, "test"), threadId, new TravelPlanState());

    private static (TravelPlanningRunner runner, CapturingNode capturingNode) SetupRunner(
        Guid threadId, ChatMessage responseMessage)
    {
        var setupNode = new SetupNode(responseMessage);
        var responseNode = new InformationResponseNode();
        var capturingNode = new CapturingNode();

        var builder = new WorkflowBuilder(setupNode);
        builder.AddEdge(setupNode, responseNode);
        builder.AddEdge(responseNode, capturingNode);

        var workflow = builder.Build();
        var session = new WorkflowSession(threadId, WorkflowState.Created, null);
        var runner = new TravelPlanningRunner(workflow, CheckpointManager.Default, session);

        return (runner, capturingNode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldSendTravelPlanExtractCommand_WhenReceivingInformationResponse()
    {
        var threadId = Guid.NewGuid();
        var message = new ChatMessage(ChatRole.User, "I'm departing from London");
        var (runner, capturingNode) = SetupRunner(threadId, message);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedCommand.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldPreserveMessageText_InTravelPlanExtractCommand()
    {
        var threadId = Guid.NewGuid();
        var message = new ChatMessage(ChatRole.User, "I'm departing from London");
        var (runner, capturingNode) = SetupRunner(threadId, message);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedCommand!.Message.Text.Should().Be("I'm departing from London");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldPreserveMessageRole_InTravelPlanExtractCommand()
    {
        var threadId = Guid.NewGuid();
        var message = new ChatMessage(ChatRole.User, "I'm departing from London");
        var (runner, capturingNode) = SetupRunner(threadId, message);

        await foreach (var _ in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None)) { }

        capturingNode.CapturedCommand!.Message.Role.Should().Be(ChatRole.User);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleAsync_ShouldNotEmitErrorEvent_WhenGivenValidInformationResponse()
    {
        var threadId = Guid.NewGuid();
        var message = new ChatMessage(ChatRole.User, "London to Paris");
        var (runner, _) = SetupRunner(threadId, message);

        var events = new List<WorkflowEvent>();
        await foreach (var evt in runner.WatchStreamAsync(CreateRequest(threadId), CancellationToken.None))
            events.Add(evt);

        events.Should().NotContain(e => e is WorkflowErrorEvent || e is ExecutorFailedEvent);
    }

    // Sets threadId and forwards InformationResponse to InformationResponseNode
    private class SetupNode(ChatMessage message)
        : Executor<TravelWorkflowRequest, InformationResponse>("SetupNode")
    {
        public override async ValueTask<InformationResponse> HandleAsync(
            TravelWorkflowRequest request, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetThreadId(request.ThreadId, cancellationToken);
            return new InformationResponse(message);
        }
    }

    private class CapturingNode() : Executor<TravelPlanExtractCommand>("CapturingNode")
    {
        public TravelPlanExtractCommand? CapturedCommand { get; private set; }

        public override async ValueTask HandleAsync(
            TravelPlanExtractCommand command, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            CapturedCommand = command;
            await context.AddEventAsync(new TravelPlanningCompleteEvent(new TravelPlanState()), cancellationToken);
        }
    }
}
