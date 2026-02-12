using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;

namespace Travel.Tests.Common;

public class FakeAgent : AIAgent
{
    private readonly Queue<AgentResponse> _responses;

    public bool WasInvoked { get; private set; }

    public FakeAgent()
    {
        _responses = new Queue<AgentResponse>();
    }

    public FakeAgent(List<AgentResponse> responses)
    {
        _responses = new Queue<AgentResponse>(responses);
    }

    public void EnqueueResponse(AgentResponse response)
    {
        _responses.Enqueue(response);
    }

    protected override Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        WasInvoked = true;

        if (_responses.Count > 0)
        {
            var response = _responses.Dequeue();
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

    protected override async ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var mockSession = new Mock<AgentSession>();
        return mockSession.Object;
    }

    protected override JsonElement SerializeSessionCore(AgentSession session, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        throw new NotImplementedException();
    }

    protected override async ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement serializedState, JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var mockSession = new Mock<AgentSession>();
        return mockSession.Object;
    }
}
