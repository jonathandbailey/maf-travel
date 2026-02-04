using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;

namespace Travel.Workflows.Tests;

public class FakeAgent : AIAgent
{
    private readonly List<AgentResponse>? _responses;
    private int _currentIndex;

    public bool WasInvoked { get; private set; }

    public FakeAgent()
    {
    }

    public FakeAgent(List<AgentResponse> responses)
    {
        _responses = responses;
        _currentIndex = 0;
    }

    protected override Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        WasInvoked = true;

        if (_responses is not null && _responses.Count > 0)
        {
            var response = _responses[Math.Min(_currentIndex, _responses.Count - 1)];
            if (_currentIndex < _responses.Count - 1)
            {
                _currentIndex++;
            }
            return Task.FromResult(response);
        }

        var responseMessages = new List<ChatMessage>
        {
            new(ChatRole.Assistant, "This is a fake planning response.")
        };

        return Task.FromResult(new AgentResponse([.. responseMessages]));
    }

    protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<AgentSession> GetNewSessionAsync(CancellationToken cancellationToken = default)
    {
        var mockSession = new Mock<AgentSession>();
        return new ValueTask<AgentSession>(mockSession.Object);
    }

        public override ValueTask<AgentSession> DeserializeSessionAsync(
            JsonElement json,
            JsonSerializerOptions? jsonOptions = null,
            CancellationToken cancellationToken = default)
        {
            var mockSession = new Mock<AgentSession>();
            return new ValueTask<AgentSession>(mockSession.Object);
        }
}
