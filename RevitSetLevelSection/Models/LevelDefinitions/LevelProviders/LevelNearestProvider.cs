using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions.LevelProviders {
    internal class LevelNearestProvider : ILevelProvider {
        public string GetLevel(double point, List<Level> levels) {
            return levels
                .OrderBy(item => Math.Abs(item.Elevation - point))
                .FirstOrDefault()?.Name;
        }
    }
}