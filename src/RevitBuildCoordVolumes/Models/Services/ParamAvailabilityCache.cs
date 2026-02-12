using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Services;

internal class ParamAvailabilityCache {
    public readonly Dictionary<string, Definition> ParamDefinitions = [];
}
