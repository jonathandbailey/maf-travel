using Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using Travel.Agents.Services;
using Travel.Workflows.Common.Infrastructure;
using Travel.Workflows.Common.Interfaces;

namespace Travel.Workflows.TravelPlanCriteria.Services;

public class WorkflowFactory(
    ICheckpointRepository checkpointRepository,
    IWorkflowSessionRepository sessionRepository,
    IAgentProvider agentProvider,
    ITravelApiClient travelApiClient,
    ILogger<TravelWorkflowService> logger) : IWorkflowFactory
{
    public Task<TravelWorkflowService> Create()
    {
        var workflowService = new TravelWorkflowService(
            checkpointRepository,
            sessionRepository,
            agentProvider,
            travelApiClient,
            logger);

        return Task.FromResult(workflowService);
    }
}
