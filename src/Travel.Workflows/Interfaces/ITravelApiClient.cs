using Travel.Agents.Dto;

namespace Travel.Workflows.Interfaces;

public interface ITravelApiClient
{
    /// <summary>
    /// Loads the travel plan associated with the given session.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the session has no linked travel plan.</exception>
    /// <exception cref="HttpRequestException">Thrown on transport or HTTP error responses.</exception>
    Task<TravelPlanState> GetPlanBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the travel plan state back to Travel.Api using <see cref="TravelPlanState.Id"/> as the plan identifier.
    /// </summary>
    Task UpdatePlanAsync(TravelPlanState plan, CancellationToken cancellationToken = default);
}
