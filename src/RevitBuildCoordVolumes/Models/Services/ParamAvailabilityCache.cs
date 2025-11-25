using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Services;
/// <summary>
/// Класс хранения кэша документов и их имен
/// </summary> 
internal class ParamAvailabilityCache {
    public readonly Dictionary<string, Definition> ParamDefinitions = [];
}
