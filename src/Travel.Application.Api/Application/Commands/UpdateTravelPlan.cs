using Infrastructure.Dto;
using MediatR;
using System.Threading;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Models.Flights;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record UpdateTravelPlanCommand(Guid UserId, Guid SessionId, TravelPlanUpdateDto TravelPlanUpdateDto) : IRequest;

public record UpdateTravelPlanFlightSearchCommand(Guid UserId, Guid SessionId, FlightSearchDto flightSearchDto) : IRequest;

public class UpdateTravelPlanHandler(ITravelPlanService travelPlanService, ISessionService sessionService) : IRequestHandler<UpdateTravelPlanCommand>
{
    public async Task Handle(UpdateTravelPlanCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionService.Get(request.UserId, request.SessionId);

        await travelPlanService.UpdateAsync(request.TravelPlanUpdateDto, request.UserId, session.TravelPlanId);
    }
}

public class UpdateTravelPlanFlightSearchHandler(ITravelPlanService travelPlanService, ISessionService sessionService) : IRequestHandler<UpdateTravelPlanFlightSearchCommand>
{
    public async Task Handle(UpdateTravelPlanFlightSearchCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionService.Get(request.UserId, request.SessionId);

        var flightOption = request.flightSearchDto.DepartureFlightOptions.First();

        var mapped = MapFlightOption(flightOption);

        await travelPlanService.SaveFlightOption(request.UserId, session.TravelPlanId, mapped);
    }

    private static FlightOption MapFlightOption(Infrastructure.Dto.FlightOptionDto flightOption)
    {
        return new FlightOption
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = new FlightEndpoint
            {
                Airport = flightOption.Departure.Airport,
                Datetime = flightOption.Departure.Datetime
            },
            Arrival = new FlightEndpoint
            {
                Airport = flightOption.Arrival.Airport,
                Datetime = flightOption.Arrival.Datetime
            },
            Duration = flightOption.Duration,
            Price = new FlightPrice
            {
                Amount = flightOption.Price.Amount,
                Currency = flightOption.Price.Currency
            }
        };
    }
}

