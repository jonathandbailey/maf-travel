using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Shared;
using Travel.Tests.Shared.Helper;
using Travel.Workflows.Dto;
using Travel.Workflows.Services;

namespace Travel.Tests.Unit.Common;

public class WorkflowMockTestHarness
{
    private readonly InMemoryCheckpointRepository _checkpointRepository = new();
    private readonly InMemoryWorkflowSessionRepository _sessionRepository = new();
    private readonly Guid _threadId = Guid.NewGuid();

    public List<WorkflowEvent> Events { get; } = [];

    public async Task<List<WorkflowEvent>> WatchStreamAsync(string message, List<AgentFactoryHelper.AgentCreateMeta> metas)
    {
        var workflowService = await Create(message, metas);

        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, message), _threadId, new TravelPlanDto());

        var events = await workflowService.WatchStreamAsync(request).ToListAsync();

        Events.AddRange(events);

        return events;
    }

    public async Task<TravelWorkflowService> Create(string message, List<AgentFactoryHelper.AgentCreateMeta> metas)
    {
        var agentProvider = new Mock<IAgentProvider>();

        foreach (var meta in metas)
        {
            var chatClient = AgentFactoryHelper.CreateMockChatClient(meta);

            var tools = meta.AgentType switch
            {
                AgentType.Planning => PlanningTools.GetDeclarationOnlyTools(),
                AgentType.Extracting => ExtractingTools.GetDeclarationOnlyTools(),
                _ => throw new ArgumentException($"Unknown agent type: {meta.AgentType}")
            };

            var agentFactory = new CustomPromptAgentFactory(chatClient, tools: tools);
            var agent = await agentFactory.CreateFromYamlAsync(StubAgentTemplate.Yaml);

            agentProvider.Setup(x => x.CreateAsync(meta.AgentType))
                .ReturnsAsync(agent);
        }

        var workflowService = new TravelWorkflowService(_checkpointRepository, _sessionRepository, agentProvider.Object, NullLogger<TravelWorkflowService>.Instance);

        return workflowService;
    }
    
}
