using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Infrastructure.Mappers;

namespace Travel.Application.Application.Queries;

public record GetFlightSearchQuery(Guid Id) : IRequest<FlightSearchResultDto>;

public class GetFlightSearchHandler(IFlightRepository flightSearchRepository) : IRequestHandler<GetFlightSearchQuery, FlightSearchResultDto>
{
    public async Task<FlightSearchResultDto> Handle(GetFlightSearchQuery request, CancellationToken cancellationToken)
    {
        var flightSearch = await flightSearchRepository.GetFlightSearch(request.Id);
       
        return flightSearch.ToDto("Flights");
    }
}