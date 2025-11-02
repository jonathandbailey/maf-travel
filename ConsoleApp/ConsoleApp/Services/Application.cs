using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Core;
using ConsoleApp.Settings;
using ConsoleApp.Workflows.ReAct;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;

namespace ConsoleApp.Services;

public class Application(IOptions<LanguageModelSettings> settings, IPromptService promptService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(
                    settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);

        var reasonAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Instructions = promptService.GetPrompt("Reason-Agent")
        });

        var actAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Instructions = promptService.GetPrompt("Act-Agent")
        });

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                continue;   
            }

            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            var reasonNode = new ReasonNode(reasonAgent);
            var actNode = new ActNode(actAgent);

            var builder = new WorkflowBuilder(reasonNode);
            builder.AddEdge(reasonNode, actNode);
            
            var workflow = await builder.BuildAsync<string>();

            var run = await InProcessExecution.StreamAsync(workflow, userInput, cancellationToken: cancellationToken);

            await foreach (var evt in run.WatchStreamAsync(cancellationToken))
            {
                if (evt is ConversationStreamingEvent { Data: not null } streamingEvent)
                {
                    var messageString = streamingEvent.Data?.ToString() ?? string.Empty;
                    Console.Write(messageString);
                }
            }

            Console.WriteLine();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}