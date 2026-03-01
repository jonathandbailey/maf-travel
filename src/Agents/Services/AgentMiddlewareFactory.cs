using Agents.Middleware;

namespace Agents.Services;

public class AgentMiddlewareFactory : IAgentMiddlewareFactory
{
    private readonly Dictionary<string, IAgentMiddleware>
        _agentMiddlewares = new();

    public AgentMiddlewareFactory(IAgentThreadMiddleware agentMiddleware,
        IAgentAgUiMiddleware agentAgUiMiddleware)
    {
        AddMiddleware(agentMiddleware);
        AddMiddleware(agentAgUiMiddleware);
    }

    private void AddMiddleware(IAgentMiddleware middleware)
    {
        _agentMiddlewares.Add(middleware.Name, middleware);
    }

    public IAgentMiddleware Get(string name)
    {
        return _agentMiddlewares[name];
    }
}

public interface IAgentMiddlewareFactory
{
    IAgentMiddleware Get(string name);
}