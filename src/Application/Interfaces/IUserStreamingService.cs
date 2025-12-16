namespace Application.Interfaces;

public interface IUserStreamingService
{
    Task Stream(Guid userId, string content, bool isEndOfStream, Guid requestId);
    Task Status(Guid userId, string content, Guid requestId, string details, string source);
    Task StreamEnd(Guid userId);
    Task Artifact(Guid userId, string key);
    Task TravelPlan(Guid userId);
}