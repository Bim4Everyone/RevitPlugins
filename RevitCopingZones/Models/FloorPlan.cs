using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitCopingZones.Models {
    internal class FloorPlan {
        public FloorPlan(string name, IEnumerable<Level> levels) {
            Name = name;
            Levels = new List<Level>(levels.OrderBy(item => item.Elevation));
            Elevation = Levels.FirstOrDefault()?.Elevation ?? double.MaxValue;
        }
        
        public string Name { get; }
        public double Elevation { get; }
        public List<Level> Levels { get; }
        
        public ViewPlan AreaPlan { get; set; }
        public bool HasAreasInPlan { get; set; }
        public bool HasViewPlan => AreaPlan != null;
        public bool CanCopyAreas => HasViewPlan;
    }
}