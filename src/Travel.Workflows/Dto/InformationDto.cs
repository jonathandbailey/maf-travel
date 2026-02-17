using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Workflows.Dto;

public record InformationRequest(string Context, List<string> Entities);

public record InformationResponse(ChatMessage Message);

public record InformationRequestDetails(string Context, List<string> Entities);

public record TravelPlanUpdateCommand(TravelPlanDto TravelPlan);

public record TravelPlanSummary(TravelPlanDto travelPlanSummary);

public record TravelPlanContextUpdated();

public record TravelPlanCompletedCommand();