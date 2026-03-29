using System.ComponentModel;
using Microsoft.Extensions.AI;
using Tools.Registry;

namespace Travel.Agents.Services;

public sealed class FlightSearchToolsHandler : IToolHandler
{
    public const string SearchFlightsToolName = "search_flights";

    public string ToolName => SearchFlightsToolName;

    private static readonly List<AITool> s_tools;

    [Description("Search for available flights between two locations on specified dates")]
    private static string SearchFlights(
        [Description("The origin airport or city code")] string origin,
        [Description("The destination airport or city code")] string destination,
        [Description("The departure date in YYYY-MM-DD format")] string departureDate,
        [Description("The return date in YYYY-MM-DD format, or null for one-way")] string? returnDate = null,
        [Description("The number of passengers")] int passengers = 1)
        => "Flight search is handled by the workflow.";

    static FlightSearchToolsHandler()
    {
        s_tools =
        [
            AIFunctionFactory.Create(SearchFlights, SearchFlightsToolName).AsDeclarationOnly()
        ];
    }

    public List<AITool> GetDeclarationOnlyTools() => s_tools;

    public IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        CancellationToken cancellationToken)
        => throw new NotSupportedException("Flight search tools are executed by workflow nodes, not the tool registry.");
}
