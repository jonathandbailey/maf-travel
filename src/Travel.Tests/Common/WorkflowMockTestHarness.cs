using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Services;

namespace Travel.Tests.Common;

public class WorkflowMockTestHarness
{
    private readonly InMemoryCheckpointRepository _checkpointRepository = new();
    private readonly Mock<ITravelPlanService> _travelPlanService;
    private readonly Guid _threadId = Guid.NewGuid();


    public WorkflowMockTestHarness()
    {
        _travelPlanService = new Mock<ITravelPlanService>();

        _travelPlanService.Setup(x => x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());
    }

    public async Task<List<WorkflowEvent>> WatchStreamAsync(string message, RequestInformationDto informationRequest, TravelPlanDto travelUpdateRequest)
    {
        var agentProvider = new Mock<IAgentProvider>();

        agentProvider.Setup(x => x.CreateAsync(AgentType.Planning))
            .ReturnsAsync(await AgentFactoryHelper.CreateMockPlanningAgent(informationRequest));

        agentProvider.Setup(x => x.CreateAsync(AgentType.Extracting))
            .ReturnsAsync(await AgentFactoryHelper.CreateMockExtractorAgent(travelUpdateRequest));

        var workflowService = new TravelWorkflowService(_travelPlanService.Object, _checkpointRepository, agentProvider.Object);

        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, message), _threadId);

        var events = await workflowService.WatchStreamAsync(request).ToListAsync();

        return events;
    }
    
}