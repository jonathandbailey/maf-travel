using System.Text.Json;

namespace Travel.Workflows.Common;

public static class Json
{
    public static readonly JsonSerializerOptions FunctionCallSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}