using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Workflows.Dto;

public record InformationRequest(string Context, List<string> Entities);

public record InformationResponse(ChatMessage Message);

public record InformationRequestDetails(string Context, List<string> Entities);

public record TravelPlanUpdateCommand(TravelPlanState TravelPlan);

public record TravelPlanSummary(TravelPlanState travelPlanSummary);

public record TravelPlanContextUpdated();

public record TravelPlanCompletedCommand();

public record RequestInformationCommand(RequestInformationDto Details);

public record TravelPlanExtractCommand(ChatMessage Message);