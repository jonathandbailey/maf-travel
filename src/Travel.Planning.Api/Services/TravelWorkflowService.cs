using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Repository;
using Travel.Workflows.Services;

namespace Travel.Planning.Api.Services;

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> Execute(WorkflowRequest request);
}

public class TravelWorkflowService(
    ICheckpointRepository repository,
    ITravelPlanService travelPlanService,
    IWorkflowFactory workflowFactory, 
    ILogger<TravelWorkflowService> logger,
    IWorkflowRepository workflowRepository) : ITravelWorkflowService
{
  
    public async Task<WorkflowResponse> Execute(WorkflowRequest request)
    {
        var workflow = await workflowFactory.Create();

        var state = await workflowRepository.LoadAsync(request.Meta.ThreadId);

        await travelPlanService.CreateTravelPlan();

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore2(repository, request.Meta.ThreadId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, logger);
    
        var response = await travelWorkflow.Execute(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, request.Meta.RawUserMessage), request.Meta.ThreadId));

        await workflowRepository.SaveAsync(request.Meta.ThreadId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new WorkflowResponse(response.State, response.Message);
    }
}