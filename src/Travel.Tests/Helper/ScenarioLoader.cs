using System.Text.Json;
using Travel.Workflows.Dto;

namespace Travel.Tests.Helper;

public record TravelPlanningScenario(string[] Messages, TravelPlanDto ExpectedTravelPlan);

public static class ScenarioLoader
{
    public static IEnumerable<TravelPlanningScenario> LoadTravelPlanningScenarios(string fileName = "TravelPlanningScenarios.json")
    {
        var currentDirectory = AppContext.BaseDirectory;
        var filePath = Path.Combine(currentDirectory, "TestData", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test data file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var scenarios = JsonSerializer.Deserialize<List<TravelPlanningScenario>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return scenarios ?? [];
    }
}
