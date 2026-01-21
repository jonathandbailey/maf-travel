using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Agents;

public static class Verify
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrWhiteSpace([NotNull] string? str, [CallerArgumentExpression(nameof(str))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(str, paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull([NotNull] object? obj, [CallerArgumentExpression(nameof(obj))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty(Guid guid, [CallerArgumentExpression(nameof(guid))] string? paramName = null)
    {
        if (guid == Guid.Empty)
        {
            throw new ArgumentException($"The GUID cannot be empty.", paramName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty(IEnumerable collection, [CallerArgumentExpression(nameof(collection))] string? paramName = null)
    {
        if (collection == null || !collection.Cast<object>().Any())
        {
            throw new ArgumentException($"The collection cannot be empty.", paramName);
        }
    }

}