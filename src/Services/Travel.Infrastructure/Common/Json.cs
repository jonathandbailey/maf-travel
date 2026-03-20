using System.Text.Json;

namespace Travel.Infrastructure.Common;

public static class Json
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}