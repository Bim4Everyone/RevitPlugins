using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCopingZones.Models {
    internal class FloorPlan {
        public FloorPlan(string name, IEnumerable<Level> levels) {
            Name = name;
            Levels = new List<Level>(levels);
        }
        
        public string Name { get; }
        public List<Level> Levels { get; }
        
        public ViewPlan AreaPlan { get; set; }
        public bool HasAreasInPlan { get; set; }
        public bool HasViewPlan => AreaPlan != null;
        public bool CanCopyAreas => HasViewPlan && !HasAreasInPlan;
    }
}