using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlightSearchResultDto = Infrastructure.Dto.FlightSearchResultDto;

namespace Travel.Workflows.Services;


public class FlightService : IFlightService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public async Task<Guid> SaveFlightSearch(FlightSearchResultDto option)
    {
        var payload = JsonSerializer.Serialize(option, SerializerOptions);

        var httpClient = new HttpClient() { BaseAddress = new Uri("https://localhost:7010/") };

        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"/api/travel/flights/search/", content);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to retrieve travel plan: {response.ReasonPhrase}");

        var responseContent = await response.Content.ReadAsStringAsync();
        var id = JsonSerializer.Deserialize<Guid>(responseContent, SerializerOptions);

        return id;
    }
}

public interface IFlightService
{
    Task<Guid> SaveFlightSearch(FlightSearchResultDto option);
}