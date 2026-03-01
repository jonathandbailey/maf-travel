using System.Text.Json;
using Travel.Agents.Dto;
using Travel.Tests.Shared;
using Travel.Tests.Shared.Helper;

namespace Travel.Tests.Unit.TestData;

public record TravelPlanningScenario(string[] Messages, TravelPlanDto ExpectedTravelPlan);

public record WorkflowRun(
    string Message,
    List<AgentFactoryHelper.AgentCreateMeta> AgentMetas);

public record PlanningWorkflowScenario(
    string ScenarioName,
    TravelPlanDto ExpectedTravelPlan,
    List<WorkflowRun> Runs);

public record ConversationAgentMeta(string Name, string? ArgumentsKey, object? Arguments);

public record ToolHandlerResultMeta(
    string Type,
    string? Message,
    string? Thought,
    string? SnapshotType,
    JsonElement? Data);

public record ConversationAgentRun(
    string Message,
    ConversationAgentMeta ConversationAgentMeta,
    List<ToolHandlerResultMeta> ToolHandlerResults);

public record ConversationAgentScenario(
    string ScenarioName,
    TravelPlanDto ExpectedTravelPlan,
    List<ConversationAgentRun> Runs);

public static class UnitTestsScenarioLoader
{
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

    public static IEnumerable<ConversationAgentScenario> LoadConversationAgentScenarios(string fileName = "ConversationAgentScenarios.json")
    {
        var currentDirectory = AppContext.BaseDirectory;
        var filePath = Path.Combine(currentDirectory, "TestData", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test data file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var scenarios = JsonSerializer.Deserialize<List<ConversationAgentScenario>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new AgentCreateMetaJsonConverter() }
        });

        return scenarios ?? [];
    }
}
