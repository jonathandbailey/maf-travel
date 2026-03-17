using Bogus;
using Travel.Experience.Mcp.Flights.Models;

namespace Travel.Experience.Mcp.Flights.Services;

public class StubFlightSearchService : IFlightSearchService
{
    private static readonly string[] Airlines = ["BA", "AA", "UA", "LH", "AF", "EK", "QR", "SQ"];

    public Task<IEnumerable<FlightOption>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken = default)
    {
        var outbound = GenerateLegs(request.Origin, request.Destination, request.DepartureDate, request.Passengers);

        IEnumerable<FlightOption> results = outbound;

        if (request.ReturnDate is { } returnDate)
        {
            var inbound = GenerateLegs(request.Destination, request.Origin, returnDate, request.Passengers);
            results = outbound.Concat(inbound);
        }

        return Task.FromResult(results);
    }

    private static IEnumerable<FlightOption> GenerateLegs(string origin, string destination, DateOnly date, int passengers)
    {
        // Seed is deterministic per route so the same query returns the same results
        var seed = Math.Abs(HashCode.Combine(origin.ToUpperInvariant(), destination.ToUpperInvariant()));
        var faker = new Faker { Random = new Randomizer(seed) };

        int count = faker.Random.Int(3, 5);

        return Enumerable.Range(0, count).Select(_ =>
        {
            var airline = faker.PickRandom(Airlines);
            var flightNumber = $"{airline}{faker.Random.Int(100, 9999)}";
            var departureHour = faker.Random.Int(5, 22);
            var durationHours = faker.Random.Int(1, 14);
            var durationMinutes = faker.Random.Int(0, 59);
            var departure = date.ToDateTime(new TimeOnly(departureHour, faker.Random.Int(0, 59)));
            var arrival = departure.AddHours(durationHours).AddMinutes(durationMinutes);
            var basePrice = faker.Random.Decimal(80, 1200);
            var pricePerPerson = Math.Round(basePrice, 2);
            var seats = faker.Random.Int(1, 150);

            return new FlightOption(
                flightNumber,
                airline,
                origin.ToUpperInvariant(),
                destination.ToUpperInvariant(),
                departure,
                arrival,
                pricePerPerson,
                seats);
        });
    }
}
