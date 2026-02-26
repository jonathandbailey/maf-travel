namespace Travel.Experience.Application.Agents.ToolHandling;

public interface IConversationToolHandlerRegistry
{
    IConversationToolHandler? GetHandler(string toolName);
}
