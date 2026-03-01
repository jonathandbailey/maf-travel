using Agents.Settings;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;


namespace Agents.Services;

public class AgentFactory : IAgentFactory
{
    private readonly IAgentMiddlewareFactory _agentMiddlewareFactory;
    private readonly ChatClient _chatClient;

    public AgentFactory(IOptions<LanguageModelSettings> settings, IAgentMiddlewareFactory agentMiddlewareFactory)
    {
        _agentMiddlewareFactory = agentMiddlewareFactory;


        var credential = new ChainedTokenCredential(
            new VisualStudioCredential(),
            new AzureCliCredential(),
            new AzureDeveloperCliCredential()
        );

        _chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint), credential)
            .GetChatClient(settings.Value.DeploymentName);
    }

    public async Task<AIAgent> Create(string template, List<AITool>? tools = null)
    {
        ArgumentNullException.ThrowIfNull(_chatClient,"ChatClient has not been created.");
        
        var agentFactory = new CustomPromptAgentFactory(_chatClient.AsIChatClient(), tools: tools);
        var agent = await agentFactory.CreateFromYamlAsync(template);

        return agent;
    }

    public async Task<AIAgent> Create(IChatClient chatClient, string template, List<AITool>? tools = null)
    {

        var agentFactory = new CustomPromptAgentFactory(chatClient, tools: tools);
        var agent = await agentFactory.CreateFromYamlAsync(template);

        return agent;
    }

    public AIAgent UseMiddleware(AIAgent agent, string name)
    {
        var middleware = _agentMiddlewareFactory.Get(name);
        
        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: middleware.RunAsync, runStreamingFunc: middleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }
}

public interface IAgentFactory
{
    AIAgent UseMiddleware(AIAgent agent, string name);

    Task<AIAgent> Create(string template, List<AITool>? tools = null);
    Task<AIAgent> Create(IChatClient chatClient, string template, List<AITool>? tools = null);
}

