using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Dto;

public record InformationRequest(ChatMessage Message);

public record InformationResponse(ChatMessage Message);

public record InformationRequestDetails(string Context, List<string> Entities);