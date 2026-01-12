using System.ClientModel;
using Agents.Dto;
using Agents.Settings;
using Azure.AI.OpenAI;
using Infrastructure.Dto;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using IAgentMemoryMiddleware = Agents.Middleware.IAgentMemoryMiddleware;
using IAgentTemplateRepository = Agents.Repository.IAgentTemplateRepository;

namespace Agents;

public class AgentFactory(
    IAgentTemplateRepository templateRepository, 
    IAgentMemoryMiddleware agentMemoryMiddleware,
    IOptions<LanguageModelSettings> settings) : IAgentFactory
{
 
    public async Task<AIAgent> CreateFlightAgent()
    {
        var template = await templateRepository.Load("Flight-Agent");

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);
     
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(FlightActionResultDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "FlightPlan",
                schemaDescription: "User Flight Options for their vacation."),
            Instructions = template
        };

        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = "flight_agent",
            ChatOptions = chatOptions
        };

        var agent = chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options: clientChatOptions);

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: agentMemoryMiddleware.RunAsync, runStreamingFunc: agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }

    public async Task<AIAgent> CreateReasonAgent()
    {
        var template = await templateRepository.Load("Reason-Agent");

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);

        var schema = AIJsonUtilities.CreateJsonSchema(typeof(ReasoningOutputDto));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "ReasoningActRequest",
                schemaDescription: "Reasoning State for Act."),
            Instructions = template
        };
      
        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = "reason_agent",
            
            ChatOptions = chatOptions
        };

        var agent = chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options: clientChatOptions);

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: agentMemoryMiddleware.RunAsync, runStreamingFunc: agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }

    public async Task<AIAgent> CreateConversationAgent(Delegate travelWorkflowService)
    {
        var template = await templateRepository.Load("Conversation-Agent");

        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);

        var clientChatOptions = new ChatClientAgentOptions
        {
            Name = "conversation_agent",
           
            ChatOptions = new ChatOptions
            {
                Tools = [AIFunctionFactory.Create(travelWorkflowService)],
                Instructions = template
            }
        };

        var agent = chatClient.AsIChatClient()
            .AsBuilder()
            .BuildAIAgent(options:clientChatOptions);

        var middlewareAgent = agent.AsBuilder()
            .Use(runFunc: null, runStreamingFunc: agentMemoryMiddleware.RunStreamingAsync)
            .Build();

        return middlewareAgent;
    }
 
}

public interface IAgentFactory
{
    Task<AIAgent> CreateConversationAgent(Delegate travelWorkflowService);
    Task<AIAgent> CreateReasonAgent();
    Task<AIAgent> CreateFlightAgent();
}

