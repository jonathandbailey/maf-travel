using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Domain;
using Travel.Application.Domain.Flights;

namespace Travel.Application.Api.Infrastructure.Mappers;

public static class TravelPlanMapper
{
    public static TravelPlanDocument ToDocument(this TravelPlan travelPlan)
    {
        return new TravelPlanDocument
        {
            Id = travelPlan.Id,
            Origin = travelPlan.Origin,
            Destination = travelPlan.Destination,
            StartDate = travelPlan.StartDate,
            EndDate = travelPlan.EndDate,
            TravelPlanStatus = travelPlan.TravelPlanStatus,
            FlightPlan = travelPlan.FlightPlan.ToDocument()
        };
    }

    public static TravelPlan ToDomain(this TravelPlanDocument document)
    {
        return new TravelPlan(
            document.Id,
            document.Origin,
            document.Destination,
            document.StartDate,
            document.EndDate,
            document.TravelPlanStatus,
            document.FlightPlan.ToDomain());
    }

    private static FlightPlanDocument ToDocument(this FlightPlan flightPlan)
    {
        return new FlightPlanDocument
        {
            FlightOptionsStatus = flightPlan.FlightOptionsStatus,
            UserFlightOptionStatus = flightPlan.UserFlightOptionStatus,
            FlightOptions = flightPlan.FlightOptions.Select(fo => fo.ToDocument()).ToList(),
            FlightOption = flightPlan.FlightOption?.ToDocument()
        };
    }

    private static FlightPlan ToDomain(this FlightPlanDocument document)
    {
        return new FlightPlan(
            document.FlightOptionsStatus,
            document.UserFlightOptionStatus,
            document.FlightOption?.ToDomain(),
            document.FlightOptions.Select(fo => fo.ToDomain()).ToList());
    }

    private static FlightOptionSearchDocument ToDocument(this FlightOptionSearch flightOptionSearch)
    {
        return new FlightOptionSearchDocument
        {
            Id = flightOptionSearch.Id
        };
    }

    private static FlightOptionSearch ToDomain(this FlightOptionSearchDocument document)
    {
        return new FlightOptionSearch(document.Id);
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
