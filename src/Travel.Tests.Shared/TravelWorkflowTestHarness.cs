
using Agents.Tools;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Tests.Shared.Helper;
using Travel.Workflows.Dto;
using Travel.Workflows.Interfaces;
using Travel.Workflows.Services;

namespace Travel.Tests.Shared;

public class TravelWorkflowTestHarness
{
    private readonly IWorkflowFactory _factory;
    private readonly Guid _threadId = Guid.NewGuid();

    private List<WorkflowEvent> _lastEvents = [];
    private readonly IAgentProvider _agentProvider;
    private readonly InMemoryCheckpointRepository _repo;
    private readonly InMemoryWorkflowSessionRepository _sessionRepo;

    public List<WorkflowEvent> Events => _lastEvents;

    public TravelWorkflowTestHarness(IWorkflowFactory factory)
    {
        _factory = factory;
        _repo = new InMemoryCheckpointRepository();
        _sessionRepo = new InMemoryWorkflowSessionRepository();

        _agentProvider = new AgentProvider(
            AgentHelper.CreateAgentFactory(),
            AgentHelper.CreateAgentTemplateRepository(),
            new ToolRegistry(
            [
                new ToolHandlerRegistration(new PlanningToolsHandler(), ["planning"]),
                new ToolHandlerRegistration(new ExtractingToolsHandler(), ["extracting"])
            ]));
    }

    public async Task<List<WorkflowEvent>> WatchStreamAsync(string message)
    {
        var service = await _factory.Create();

        var request = new TravelWorkflowRequest(
            new ChatMessage(ChatRole.User, message),
            _threadId);

        _lastEvents = await service.WatchStreamAsync(request).ToListAsync();

        Events.AddRange(_lastEvents);

        return _lastEvents;
    }
}