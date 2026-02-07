using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Services;

public class WorkflowTools : IWorkflowTools
{
    private readonly List<AITool> _tools = [];

    public void Add(AITool tool)
    {
        _tools.Add(tool);
    }

    public AITool Get(string name)
    {
         return _tools.First(t => t.Name == name);
    }
}

public interface IWorkflowTools
{
    void Add(AITool tool);
    AITool Get(string name);
}