using System.Net.Http.Json;
using Travel.Agents.Dto;
using Travel.Workflows.Interfaces;

namespace Travel.Workflows.TravelPlanCriteria.Services;

public class TravelApiClient(HttpClient httpClient) : ITravelApiClient
{
    public async Task<TravelPlanState> GetPlanBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await httpClient.GetFromJsonAsync<SessionResponse>(
            $"/sessions/{sessionId}", cancellationToken);

        if (session is null)
            throw new InvalidOperationException($"Session '{sessionId}' was not found.");

        if (session.TravelPlanId is null)
            throw new InvalidOperationException($"Session '{sessionId}' has no linked travel plan.");

        var plan = await httpClient.GetFromJsonAsync<TravelPlanResponse>(
            $"api/travel/plans/{session.TravelPlanId}", cancellationToken);

        if (plan is null)
            throw new InvalidOperationException($"Travel plan '{session.TravelPlanId}' was not found.");

        return new TravelPlanState(
            id: plan.Id,
            origin: plan.Origin,
            destination: plan.Destination,
            startDate: plan.StartDate,
            endDate: plan.EndDate,
            numberOfTravelers: plan.NumberOfTravelers);
    }

    public async Task UpdatePlanAsync(TravelPlanState plan, CancellationToken cancellationToken = default)
    {
        var request = new UpdateTravelPlanRequest(
            plan.Origin,
            plan.Destination,
            plan.NumberOfTravelers,
            plan.StartDate,
            plan.EndDate);

        var response = await httpClient.PutAsJsonAsync(
            $"api/travel/plans/{plan.Id}", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private record SessionResponse(Guid Id, DateTime CreatedAt, Guid? TravelPlanId);

    private record TravelPlanResponse(
        Guid Id,
        string? Origin,
        string? Destination,
        int? NumberOfTravelers,
        DateTime? StartDate,
        DateTime? EndDate);

    private record UpdateTravelPlanRequest(
        string? Origin,
        string? Destination,
        int? NumberOfTravelers,
        DateTime? StartDate,
        DateTime? EndDate);
}
