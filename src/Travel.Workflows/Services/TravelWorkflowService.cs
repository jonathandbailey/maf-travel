using Agents.Services;
using Infrastructure.Repository;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    IAgentFactory agentFactory, 
    ITravelPlanService travelPlanService, 
    IAgentTemplateRepository agentTemplateRepository)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var workflowFactory = new WorkflowFactory(travelPlanService);

        var planningAgentTemplate = await agentTemplateRepository.LoadAsync("planning.yaml");

        var extractingAgentTemplate = await agentTemplateRepository.LoadAsync("extracting.yaml");

        var planningAgent = await agentFactory.Create(planningAgentTemplate, PlanningTools.GetDeclarationOnlyTools());

        var extractingAgent =
            await agentFactory.Create(extractingAgentTemplate, ExtractingTools.GetDeclarationOnlyTools());

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var travelPlanningWorkflow = new TravelPlanningWorkflow();

        var message = new ChatMessage(ChatRole.User, request.Message);

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(workflow, checkpointManager, message))
        {
            yield return evt;
        }
    }
}