using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.Repositories {
    internal interface IZoneRepository {
        Transform Transform { get; }
        List<ZoneInfo> GetZones();
    }
}