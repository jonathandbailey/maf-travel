using Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using Travel.Agents.Services;
using Travel.Workflows.Infrastructure;
using Travel.Workflows.Interfaces;

namespace Travel.Workflows.Services;

public class WorkflowFactory(
    ICheckpointRepository checkpointRepository,
    IWorkflowSessionRepository sessionRepository,
    IAgentProvider agentProvider,
    ITravelPlanRepository travelPlanRepository,
    ILogger<TravelWorkflowService> logger) : IWorkflowFactory
{
    public Task<TravelWorkflowService> Create()
    {
        var workflowService = new TravelWorkflowService(
            checkpointRepository,
            sessionRepository,
            agentProvider,
            travelPlanRepository,
            logger);

        return Task.FromResult(workflowService);
    }
}
