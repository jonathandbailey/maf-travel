using Application.Agents;
using Application.Observability;
using Application.Workflows.Conversations.Dto;
using Application.Workflows.Conversations.Nodes;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations;

public class ConversationWorkflow(IAgent reasonAgent, IAgent actAgent, IWorkflowManager workflowManager) : WorkflowBase<ChatMessage>
{
    public async Task<WorkflowResponse> Execute(ChatMessage message)
    {
        var inputPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);

        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, inputPort);
        builder.AddEdge(inputPort, actNode);
        builder.AddEdge(actNode, reasonNode);

        var workflow = await builder.BuildAsync<ChatMessage>();

        var run = await CreateStreamingRun(workflow, message);

        await foreach (var evt in run.Run.WatchStreamAsync())
        {
            if (workflowManager.State == WorkflowState.Initialized)
            {
                workflowManager.Executing();
            }
            
            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

                if (checkpoint != null)
                {
                    workflowManager.CheckpointInfo = checkpoint;
                }
            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                switch (workflowManager.State)
                {
                    case WorkflowState.Executing:
                    {
                        return HandleRequestForUserInput(requestInfoEvent);
                    }
                    case WorkflowState.WaitingForUserInput:
                    {
                        var resp = requestInfoEvent.Request.CreateResponse(new UserResponse(message.Text));

                        workflowManager.Executing();
                        await run.Run.SendResponseAsync(resp);
                        break;
                    }
                }
            }
        }

        return new WorkflowResponse(WorkflowResponseState.Completed, string.Empty);
    }

    private async Task<Checkpointed<StreamingRun>> CreateStreamingRun(Workflow<ChatMessage> workflow,
        ChatMessage message)
    {
        using var workflowActivity = Telemetry.StarActivity("Workflow-[create-run]");

        workflowActivity?.SetTag("State:", workflowManager.State);

        switch (workflowManager.State)
        {
            case WorkflowState.Initialized:
                return await StartStreamingRun(workflow, message);
            case WorkflowState.WaitingForUserInput:
                return await ResumeStreamingRun(workflow, message);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<Checkpointed<StreamingRun>> StartStreamingRun(Workflow<ChatMessage> workflow,
        ChatMessage message)
    {
        using var workflowActivity = Telemetry.StarActivity("Workflow-[start]");


        return await InProcessExecution.StreamAsync(workflow, message, workflowManager.CheckpointManager);
    }

    private async Task<Checkpointed<StreamingRun>> ResumeStreamingRun(Workflow<ChatMessage> workflow,
        ChatMessage message)
    {
        var run = await InProcessExecution.ResumeStreamAsync(workflow, workflowManager.CheckpointInfo, workflowManager.CheckpointManager,
            workflowManager.CheckpointInfo.RunId);

        using var workflowActivity = Telemetry.StarActivity("Workflow-[resume]");

        workflowActivity?.SetTag("RunId:", workflowManager.CheckpointInfo.RunId);
        workflowActivity?.SetTag("CheckpointId:", workflowManager.CheckpointInfo.CheckpointId);

        return run;

    }
    private WorkflowResponse HandleRequestForUserInput(RequestInfoEvent requestInfoEvent)
    {
        var data = requestInfoEvent.Data as ExternalRequest;

        if (data?.Data == null)
        {
            return new WorkflowResponse(WorkflowResponseState.Error,
                "Invalid request event: missing data");
        }

        if (data.Data.AsType(typeof(UserRequest)) is not UserRequest userRequest)
        {
            return new WorkflowResponse(WorkflowResponseState.Error,
                "Invalid request event: unable to parse UserRequest");
        }

        if (string.IsNullOrWhiteSpace(userRequest.Message))
        {
            return new WorkflowResponse(WorkflowResponseState.Error,
                "Invalid request event: UserRequest message is empty");
        }

        workflowManager.WaitingForUserInput();

        return new WorkflowResponse(WorkflowResponseState.UserInputRequired, userRequest.Message);
    }
}

public enum WorkflowState
{
    Initialized,
    Executing,
    WaitingForUserInput,
    Completed,
    Error
}