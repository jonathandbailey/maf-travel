using Travel.Agents.Dto;

namespace Travel.Workflows.Common.Extensions;

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

    public static void ApplyPatch(this TravelPlanState target, TravelPlanData patch)
    {
        if (patch.Origin != null) target.Origin = patch.Origin;
        if (patch.Destination != null) target.Destination = patch.Destination;
        if (patch.StartDate != null) target.StartDate = patch.StartDate;
        if (patch.EndDate != null) target.EndDate = patch.EndDate;
        if (patch.NumberOfTravelers != null) target.NumberOfTravelers = patch.NumberOfTravelers;
    }
}