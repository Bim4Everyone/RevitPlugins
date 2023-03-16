using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions.LevelProviders {
    internal class LevelBottomProvider : ILevelProvider {
        public string GetLevel(double point, List<Level> levels) {
            return levels.OrderBy(item => item.Elevation)
                .Where(item => item.Elevation < point)
                .FirstOrDefault()?.Name;

        }
    }
}