using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Services;

namespace Travel.Tests.Common;

public class WorkflowMockTestHarness2
{
    private readonly InMemoryCheckpointRepository _checkpointRepository = new();
    private readonly Mock<ITravelPlanService> _travelPlanService;
    private readonly Guid _threadId = Guid.NewGuid();
    private CheckpointInfo? _checkpointInfo;

    private List<WorkflowEvent> _lastEvents = [];
    public List<WorkflowEvent> Events => _lastEvents;


    public WorkflowMockTestHarness2()
    {
         _travelPlanService = new Mock<ITravelPlanService>();

        _travelPlanService.Setup(x => x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());
    }

    public async Task<List<WorkflowEvent>> WatchStreamAsync(string message, List<AgentFactoryHelper.AgentCreateMeta> metas)
    {
        var agentProvider = new Mock<IAgentProvider>();

        foreach (var meta in metas)
        {
            var chatClient = AgentFactoryHelper.CreateMockChatClient(meta);

            var agent = await AgentFactoryHelper.Create(meta.AgentType, chatClient);
            
            agentProvider.Setup(x => x.CreateAsync(meta.AgentType))
                .ReturnsAsync(agent);
        }

        var workflowService = new TravelWorkflowService(_travelPlanService.Object, _checkpointRepository, agentProvider.Object);

        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, message), _threadId, _checkpointInfo);

        var events = await workflowService.WatchStreamAsync(request).ToListAsync();

        _checkpointInfo = events.GetCheckpointInfo();

        Events.AddRange(events);

        return events;
    }
    
}