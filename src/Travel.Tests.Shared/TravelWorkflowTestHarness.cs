
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Common;
using Travel.Tests.Shared.Helper;
using Travel.Workflows.Dto;
using Travel.Workflows.Services;

namespace Travel.Tests.Shared;

public class TravelWorkflowTestHarness
{
    private readonly Guid _threadId  = Guid.NewGuid();
    private CheckpointInfo? _lastCheckpoint;
    
    private List<WorkflowEvent> _lastEvents = [];
    private readonly IAgentProvider _agentProvider;
    private readonly Mock<ITravelPlanService> _mockSvc;
    private readonly InMemoryCheckpointRepository _repo;

    public List<WorkflowEvent> Events => _lastEvents;

    public TravelWorkflowTestHarness()
    {
        _repo = new InMemoryCheckpointRepository();
        _mockSvc = new Mock<ITravelPlanService>();
        _mockSvc.Setup(x => x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());

         _agentProvider = new AgentProvider(
            AgentHelper.CreateAgentFactory(),
            AgentHelper.CreateAgentTemplateRepository());

    }
    public async Task<List<WorkflowEvent>> WatchStreamAsync(string message)
    {
        var service = new TravelWorkflowService(_mockSvc.Object, _repo, _agentProvider);

        var request = new TravelWorkflowRequest(
            new ChatMessage(ChatRole.User, message),
            _threadId,
            _lastCheckpoint);

        _lastEvents = await service.WatchStreamAsync(request).ToListAsync();

        _lastCheckpoint = _lastEvents.GetCheckpointInfo();

        Events.AddRange(_lastEvents);

        return _lastEvents;
    }

   
}