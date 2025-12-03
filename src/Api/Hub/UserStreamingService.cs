using Api.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Api.Hub;

public class UserStreamingService(IHubContext<UserHub> hub, IUserConnectionManager userConnectionManager, IOptions<HubSettings> hubSettings) : IUserStreamingService
{
    public async Task Stream(Guid userId, string content, bool isEndOfStream, Guid requestId)
    {
        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync(hubSettings.Value.PromptChannel, new UserResponseDto() { Message = content, IsEndOfStream = isEndOfStream, Id = requestId});
        }
    }

    public async Task StreamEnd(Guid userId)
    {
        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync(hubSettings.Value.PromptChannel, new UserResponseDto() {IsEndOfStream = true});
        }
    }

    public async Task Status(Guid userId, string content, Guid requestId)
    {
        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync("status", new UserResponseDto() { Message = content, Id = requestId});
        }
    }

    public async Task Artifact(Guid userId, string key)
    {
        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync("artifact", new ArtifactStatusDto(key));
        }
    }
}

