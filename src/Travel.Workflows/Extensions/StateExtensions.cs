namespace Travel.Workflows.Extensions;

public static class StateExtensions
{
    public static void ApplyPatch<T>(this T target, T patch) where T : class
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var patchValue = prop.GetValue(patch);
            if (patchValue != null)
            {
                prop.SetValue(target, patchValue);
            }
        }
    }
}