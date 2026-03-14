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
