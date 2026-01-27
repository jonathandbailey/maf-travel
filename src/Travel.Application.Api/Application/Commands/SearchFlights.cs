using MediatR;
using Travel.Application.Api.Domain.Flights;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Api.Infrastructure.Mappers;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record SearchFlightsCommand(
    Guid UserId,
    Guid ThreadId,
    string Origin,
    string Destination,
    DateTimeOffset DepartureDate,
    DateTimeOffset ReturnDate) : IRequest<FlightSearchResultDto>;

public class SearchFlightsCommandHandler(IFlightSearchService flightSearchService, ITravelPlanRepository travelPlanRepository, IFlightRepository flightRepository, ISessionRepository sessionRepository) : IRequestHandler<SearchFlightsCommand, FlightSearchResultDto>
{
    public async Task<FlightSearchResultDto> Handle(SearchFlightsCommand request, CancellationToken cancellationToken)
    {
        var result = await flightSearchService.SearchFlights(
            request.Origin,
            request.Destination,
            request.DepartureDate,
            request.ReturnDate);

        await flightRepository.SaveFlightSearch(result);

        var session = await sessionRepository.LoadAsync(request.UserId, request.ThreadId);

        var travelPlan = await travelPlanRepository.LoadAsync(request.UserId, session.TravelPlanId);

        travelPlan.AddFlightSearchOption(new FlightOptionSearch(result.Id));

        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);


        return result.ToDto("Flights");
    }
}