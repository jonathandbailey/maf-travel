using Api.Extensions;
using Application.Users;
using System.Text;
using System.Text.Json;

namespace Api.Middleware;

public sealed class ExecutionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IExecutionContextAccessor accessor)
    {
        var userId = context.User.Id();

        if (context.Request.Path.StartsWithSegments("/ag-ui"))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);

                    var root = doc.RootElement;

                    if (root.TryGetProperty("threadId", out var threadIdProp) &&
                        root.TryGetProperty("runId", out var runIdProp))
                    {
                        var threadId = threadIdProp.GetString();
                        var runId = runIdProp.GetString();

                        if (!string.IsNullOrEmpty(threadId) &&
                            !string.IsNullOrEmpty(runId))
                        {
                            accessor.Initialize(userId, Guid.Parse(threadId), Guid.Parse(runId));
                        }
                    }
                }
                catch (JsonException)
                {
                    // Not AGUI JSON — ignore
                }
            }
        }

        await next(context);
    }
}
