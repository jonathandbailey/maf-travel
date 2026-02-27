using Microsoft.Extensions.AI;

namespace Travel.Experience.Application.Agents.ToolHandling;

public interface IToolRegistry
{
    IToolHandler? GetHandler(string toolName);
    List<AITool> GetAllDeclarationOnlyTools();
}
