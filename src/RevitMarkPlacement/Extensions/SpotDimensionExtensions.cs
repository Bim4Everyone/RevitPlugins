using System;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Extensions;

internal static class SpotDimensionExtensions {
#if REVIT_2022_OR_GREATER
    public static bool HasLeader(this SpotDimension spotDimension) {
        return spotDimension.HasLeader;
    }
#else
    public static bool HasLeader(this SpotDimension spotDimension) {
        return true;
    }
#endif

    public static bool FilterSpotDimensions(this SpotDimension spotDimension, string filterSpotName = null) {
        return spotDimension.HasLeader()
               && (string.IsNullOrEmpty(filterSpotName)
                   || spotDimension.Name.EndsWith(filterSpotName, StringComparison.OrdinalIgnoreCase));
    }
}
