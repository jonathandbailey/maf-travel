namespace Travel.Application.Api.Domain;

public class Location(string airportName, string airportCode, string city, string country)
{
    public string AirportName { get; } = airportName;

    public string AirportCode { get; } = airportCode;

    public string City { get; } = city;
    public string Country { get; } = country;
}