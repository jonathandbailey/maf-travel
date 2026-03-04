namespace Travel.Tests.Unit.Common;

/// <summary>
/// Provides a minimal stub YAML template for unit tests where the agent's IChatClient is mocked
/// and the actual prompt content is never used.
/// </summary>
public static class StubAgentTemplate
{
    public const string Yaml = """
        kind: Prompt
        name: stub_agent
        description: Stub agent for unit tests
        instructions: |
         Stub
        """;
}
