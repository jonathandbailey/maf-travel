using Travel.Application.Api.Domain.Flights;
using Travel.Application.Api.Dto;

namespace Travel.Application.Api.Infrastructure.Mappers;

public static class FlightSearchDtoMapper
{
    public static FlightSearchResultDto ToDto(this FlightSearch flightSearch, string artifactKey)
    {
        return new FlightSearchResultDto
        {
            Id = flightSearch.Id,
            ArtifactKey = artifactKey,
            DepartureFlightOptions = flightSearch.DepartureFlightOptions.Select(fo => fo.ToDto()).ToList(),
            ReturnFlightOptions = flightSearch.ReturnFlightOptions.Select(fo => fo.ToDto()).ToList()
        };
    }

    public static FlightSearch ToDomain(this FlightSearchResultDto resultDto)
    {
        return new FlightSearch(
            resultDto.DepartureFlightOptions.Select(fo => fo.ToDomain()).ToList(),
            resultDto.ReturnFlightOptions.Select(fo => fo.ToDomain()).ToList());
    }

    private static FlightOptionDto ToDto(this FlightOption flightOption)
    {
        return new FlightOptionDto
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = flightOption.Departure.ToDto(),
            Arrival = flightOption.Arrival.ToDto(),
            Duration = flightOption.Duration,
            Price = flightOption.Price.ToDto()
        };
    }

    private static FlightOption ToDomain(this FlightOptionDto dto)
    {
        return new FlightOption
        {
            Airline = dto.Airline,
            FlightNumber = dto.FlightNumber,
            Departure = dto.Departure.ToDomain(),
            Arrival = dto.Arrival.ToDomain(),
            Duration = dto.Duration,
            Price = dto.Price.ToDomain()
        };
    }

    private static FlightEndpointDto ToDto(this FlightEndpoint flightEndpoint)
    {
        return new FlightEndpointDto
        {
            Airport = flightEndpoint.Airport,
            AirportCode =flightEndpoint.AirportCode,
            Datetime = flightEndpoint.Datetime
        };
    }

    private static FlightEndpoint ToDomain(this FlightEndpointDto dto)
    {
        return new FlightEndpoint
        {
            Airport = dto.Airport,
            Datetime = dto.Datetime,
            AirportCode = dto.AirportCode
        };
    }

    private static PriceDto ToDto(this FlightPrice flightPrice)
    {
        return new PriceDto
        {
            Amount = flightPrice.Amount,
            Currency = flightPrice.Currency
        };
    }

    private static FlightPrice ToDomain(this PriceDto dto)
    {
        return new FlightPrice
        {
            Amount = dto.Amount,
            Currency = dto.Currency
        };
    }
}
