using Microsoft.Agents.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Helper;
using TravelPlanDto = Travel.Workflows.Dto.TravelPlanDto;

namespace Travel.Tests.Common;

public class AgentScenarioBuilder
{
    private FakeAgent? _extractingAgent;
    private FakeAgent? _planningAgent;

    public AgentScenarioBuilder WithExtractor(TravelPlanDto travelUpdateRequest)
    {
        var agent = new FakeAgent();
        _extractingAgent = (FakeAgent)agent.UpdateTravelPlan(travelUpdateRequest);
        return this;
    }

    public AgentScenarioBuilder WithPlanner(RequestInformationDto informationRequest)
    {
        var agent = new FakeAgent();
        _planningAgent = (FakeAgent)agent.InformationRequest(informationRequest);
        return this;
    }

    public AgentScenarioBuilder WithPlanningComplete()
    {
        var agent = new FakeAgent();
        _planningAgent = (FakeAgent)agent.PlanningComplete();
        return this;
    }

    public AgentScenarioBuilder WithPlannerFinalize()
    {
        if (_planningAgent == null)
        {
            _planningAgent = new FakeAgent();
        }
        _planningAgent.FinalizeTravelPlan();
        return this;
    }

    public IAgentProvider BuildProvider()
    {
        var agentProvider = new Mock<IAgentProvider>();

        if (_extractingAgent != null)
        {
            agentProvider.Setup(x => x.CreateAsync(AgentType.Extracting))
                .ReturnsAsync(_extractingAgent);
        }

        if (_planningAgent != null)
        {
            agentProvider.Setup(x => x.CreateAsync(AgentType.Planning))
                .ReturnsAsync(_planningAgent);
        }

        return agentProvider.Object;
    }
}
