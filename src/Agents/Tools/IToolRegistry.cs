using Microsoft.Extensions.AI;

namespace Agents.Tools;

public interface IToolRegistry
{
    IToolHandler? GetHandler(string toolName);
    List<AITool> GetAllDeclarationOnlyTools();
    List<AITool> GetDeclarationOnlyTools(string group);
}
