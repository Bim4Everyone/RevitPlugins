using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models {
    internal class ZoneInfo {
        public Area Area { get; set; }
        public Level Level { get; set; }
        public Solid Solid { get; set; }
    }
}