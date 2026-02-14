using System.Text.Json;
using Travel.Tests.Common;
using Travel.Workflows.Dto;

namespace Travel.Tests.Helper;

public record TravelPlanningScenario(string[] Messages, TravelPlanDto ExpectedTravelPlan);

public record WorkflowRun(
    string Message,
    List<AgentFactoryHelper.AgentCreateMeta> AgentMetas);

public record PlanningWorkflowScenario(
    string ScenarioName,
    TravelPlanDto ExpectedTravelPlan,
    List<WorkflowRun> Runs);

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

    public static IEnumerable<PlanningWorkflowScenario> LoadPlanningWorkflowScenarios(string fileName = "PlanningWorkflowScenarios.json")
    {
        var currentDirectory = AppContext.BaseDirectory;
        var filePath = Path.Combine(currentDirectory, "TestData", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test data file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var scenarios = JsonSerializer.Deserialize<List<PlanningWorkflowScenario>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new AgentCreateMetaJsonConverter() }
        });

        return scenarios ?? [];
    }
}
