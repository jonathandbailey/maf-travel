using Travel.Application.Api.Domain.Flights;
using Travel.Application.Api.Infrastructure.Documents;

namespace Travel.Application.Api.Infrastructure.Mappers;

public static class FlightSearchMapper
{
    public static FlightSearchDocument ToDocument(this FlightSearch flightSearch)
    {
        return new FlightSearchDocument
        {
            Id = flightSearch.Id,
            
            DepartureFlightOptions = flightSearch.DepartureFlightOptions.Select(fo => fo.ToDocument()).ToList(),
            ReturnFlightOptions = flightSearch.ReturnFlightOptions.Select(fo => fo.ToDocument()).ToList()
        };
    }

    public static FlightSearch ToDomain(this FlightSearchDocument document)
    {
        return new FlightSearch(
            document.DepartureFlightOptions.Select(fo => fo.ToDomain()).ToList(),
            document.ReturnFlightOptions.Select(fo => fo.ToDomain()).ToList());
    }

    private static FlightOptionDocument ToDocument(this FlightOption flightOption)
    {
        return new FlightOptionDocument
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = flightOption.Departure.ToDocument(),
            Arrival = flightOption.Arrival.ToDocument(),
            Duration = flightOption.Duration,
            Price = flightOption.Price.ToDocument()
        };
    }

    private static FlightOption ToDomain(this FlightOptionDocument document)
    {
        return new FlightOption
        {
            Airline = document.Airline,
            FlightNumber = document.FlightNumber,
            Departure = document.Departure.ToDomain(),
            Arrival = document.Arrival.ToDomain(),
            Duration = document.Duration,
            Price = document.Price.ToDomain()
        };
    }

    private static FlightEndpointDocument ToDocument(this FlightEndpoint flightEndpoint)
    {
        return new FlightEndpointDocument
        {
            Airport = flightEndpoint.Airport,
            Time = flightEndpoint.Datetime
        };
    }

    private static FlightEndpoint ToDomain(this FlightEndpointDocument document)
    {
        return new FlightEndpoint
        {
            Airport = document.Airport,
            Datetime = document.Time
        };
    }

    private static FlightPriceDocument ToDocument(this FlightPrice flightPrice)
    {
        return new FlightPriceDocument
        {
            Amount = flightPrice.Amount,
            Currency = flightPrice.Currency
        };
    }

    private static FlightPrice ToDomain(this FlightPriceDocument document)
    {
        return new FlightPrice
        {
            Amount = document.Amount,
            Currency = document.Currency
        };
    }
}
