namespace Travel.Application.Api.Infrastructure.Seed;

using System.Collections.Immutable;
using Domain;

public static class DataRegistry
{
    public static readonly ImmutableList<Location> EuropeanCities = ImmutableList.Create(
        
        // United Kingdom
        new Location("Heathrow Airport", "LHR", "London", "United Kingdom"),
        new Location("Gatwick Airport", "LGW", "London", "United Kingdom"),
        new Location("Manchester Airport", "MAN", "Manchester", "United Kingdom"),
        new Location("Edinburgh Airport", "EDI", "Edinburgh", "United Kingdom"),

        // France
        new Location("Charles de Gaulle Airport", "CDG", "Paris", "France"),
        new Location("Orly Airport", "ORY", "Paris", "France"),
        new Location("Nice Côte d'Azur Airport", "NCE", "Nice", "France"),
        new Location("Lyon–Saint-Exupéry Airport", "LYS", "Lyon", "France"),

        // Germany
        new Location("Frankfurt am Main Airport", "FRA", "Frankfurt", "Germany"),
        new Location("Munich Airport", "MUC", "Munich", "Germany"),
        new Location("Berlin Brandenburg Airport", "BER", "Berlin", "Germany"),
        new Location("Düsseldorf Airport", "DUS", "Düsseldorf", "Germany"),

        // Spain
        new Location("Adolfo Suárez Madrid–Barajas Airport", "MAD", "Madrid", "Spain"),
        new Location("Josep Tarradellas Barcelona–El Prat Airport", "BCN", "Barcelona", "Spain"),
        new Location("Málaga Airport", "AGP", "Málaga", "Spain"),
        new Location("Palma de Mallorca Airport", "PMI", "Palma de Mallorca", "Spain"),

        // Italy
        new Location("Leonardo da Vinci–Fiumicino Airport", "FCO", "Rome", "Italy"),
        new Location("Malpensa Airport", "MXP", "Milan", "Italy"),
        new Location("Venice Marco Polo Airport", "VCE", "Venice", "Italy"),

        // Other Major European Hubs
        new Location("Amsterdam Airport Schiphol", "AMS", "Amsterdam", "Netherlands"),
        new Location("Dublin Airport", "DUB", "Dublin", "Ireland"),
        new Location("Zürich Airport", "ZRH", "Zürich", "Switzerland"),
        new Location("Vienna International Airport", "VIE", "Vienna", "Austria"),
        new Location("Copenhagen Airport", "CPH", "Copenhagen", "Denmark"),
        new Location("Lisbon Airport", "LIS", "Lisbon", "Portugal"),
        new Location("Brussels Airport", "BRU", "Brussels", "Belgium"),
        new Location("Stockholm Arlanda Airport", "ARN", "Stockholm", "Sweden"),
        new Location("Oslo Airport, Gardermoen", "OSL", "Oslo", "Norway"),
        new Location("Athens International Airport", "ATH", "Athens", "Greece"),
        new Location("Warsaw Chopin Airport", "WAW", "Warsaw", "Poland"),
        new Location("Helsinki Airport", "HEL", "Helsinki", "Finland"),
        new Location("Václav Havel Airport Prague", "PRG", "Prague", "Czech Republic")
    );

 

public record Airline(string Name, string Code, string Country, bool IsLowCost);


    public static readonly ImmutableList<Airline> EuropeanAirlines = ImmutableList.Create(
        // United Kingdom
        new Airline("British Airways", "BA", "United Kingdom", false),
        new Airline("easyJet", "U2", "United Kingdom", true),
        new Airline("Virgin Atlantic", "VS", "United Kingdom", false),
        new Airline("Jet2", "LS", "United Kingdom", true),

        // Germany
        new Airline("Lufthansa", "LH", "Germany", false),
        new Airline("Eurowings", "EW", "Germany", true),
        new Airline("Condor", "DE", "Germany", false),

        // France
        new Airline("Air France", "AF", "France", false),
        new Airline("Transavia France", "TO", "France", true),

        // Ireland
        new Airline("Ryanair", "FR", "Ireland", true),
        new Airline("Aer Lingus", "EI", "Ireland", false),

        // Netherlands & Belgium
        new Airline("KLM Royal Dutch Airlines", "KL", "Netherlands", false),
        new Airline("Brussels Airlines", "SN", "Belgium", false),

        // Spain & Portugal
        new Airline("Iberia", "IB", "Spain", false),
        new Airline("Vueling", "VY", "Spain", true),
        new Airline("TAP Air Portugal", "TP", "Portugal", false),

        // Italy
        new Airline("ITA Airways", "AZ", "Italy", false),
        new Airline("Neos", "NO", "Italy", false),

        // Scandinavia & Nordics
        new Airline("SAS Scandinavian Airlines", "SK", "Sweden/Norway/Denmark", false),
        new Airline("Finnair", "AY", "Finland", false),
        new Airline("Norwegian Air", "DY", "Norway", true),
        new Airline("Icelandair", "FI", "Iceland", false),

        // Central & Eastern Europe
        new Airline("Wizz Air", "W6", "Hungary", true),
        new Airline("LOT Polish Airlines", "LO", "Poland", false),
        new Airline("Austrian Airlines", "OS", "Austria", false),
        new Airline("Swiss International Air Lines", "LX", "Switzerland", false),
        new Airline("Turkish Airlines", "TK", "Turkey", false),
        new Airline("Aegean Airlines", "A3", "Greece", false)
    );

}