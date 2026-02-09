using Microsoft.Agents.AI;
using Microsoft.Agents.ObjectModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.PowerFx;

namespace Agents.Services;

public class CustomPromptAgentFactory : PromptAgentFactory
{
    /// <summary>
    /// Creates a new instance of the <see cref="CustomPromptAgentFactory"/> class.
    /// </summary>
    public CustomPromptAgentFactory(
        IChatClient chatClient, 
        IList<AITool>? tools,
        IList<AIFunction>? functions = null, 
        RecalcEngine? engine = null, 
        IConfiguration? configuration = null, 
        ILoggerFactory? loggerFactory = null) : base(engine, configuration)
    {
   
        _chatClient = chatClient;
        _tools = tools;
        _functions = functions;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public override Task<AIAgent?> TryCreateAsync(GptComponentMetadata promptAgent, CancellationToken cancellationToken = default)
    {
    
        var options = new ChatClientAgentOptions()
        {
            Name = promptAgent.Name,
            Description = promptAgent.Description,
            ChatOptions = promptAgent.GetChatOptions(this.Engine, this._functions),
        };

        options.ChatOptions?.Tools = _tools;

        var agent = new ChatClientAgent(this._chatClient, options, this._loggerFactory);

        return Task.FromResult<AIAgent?>(agent);
    }

    #region private
    private readonly IChatClient _chatClient;
    private readonly IList<AITool>? _tools;
    private readonly IList<AIFunction>? _functions;
    private readonly ILoggerFactory? _loggerFactory;
    #endregion
}