using System.Text.Json;
using Agents.Extensions;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Agents.Services;

namespace Travel.Agents.Extensions;


public class PlanningService(IAgentFactory agentFactory) : IPlanningService
{
    public async Task<AgentResponse> RunPlanningAsync(TravelPlanDto travelPlan, string observation, string threadId, CancellationToken cancellationToken)
    {
        var agent = await agentFactory.Create("planning_agent_ex", tools: PlanningTools.GetDeclarationOnlyTools());

        agentFactory.UseMiddleware(agent, "agent-thread");

        var serialized = JsonSerializer.Serialize(travelPlan);

        var template = $"Observation: {observation}\nTravelPlanSummary : {serialized}";

        var message = new ChatMessage(ChatRole.User, template);

        var agentRunOptions = new ChatClientAgentRunOptions();

        agentRunOptions.AddThreadId(threadId);

        var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: cancellationToken);

        return response;
    }
}

public interface IPlanningService
{
    Task<AgentResponse> RunPlanningAsync(TravelPlanDto travelPlan, string observation, string threadId, CancellationToken cancellationToken);
}