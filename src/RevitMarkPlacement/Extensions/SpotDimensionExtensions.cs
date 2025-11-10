using System;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Extensions;

internal static class SpotDimensionExtensions {
    public static bool FilterSpotDimensions(this SpotDimension spotDimension, string filterSpotName = null) {
        return spotDimension.HasLeader
               && spotDimension.LeaderHasShoulder
               && (string.IsNullOrEmpty(filterSpotName)
                   || spotDimension.Name.EndsWith(filterSpotName, StringComparison.OrdinalIgnoreCase));
    }
}
