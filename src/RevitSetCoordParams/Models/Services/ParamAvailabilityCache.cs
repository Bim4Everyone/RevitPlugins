using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Services;

internal class ParamAvailabilityCache {
    public readonly Dictionary<string, Definition> ParamDefinitions = [];
}
