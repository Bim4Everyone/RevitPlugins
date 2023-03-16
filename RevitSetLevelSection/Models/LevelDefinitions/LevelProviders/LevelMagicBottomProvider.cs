using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions.LevelProviders {
    public class LevelMagicBottomProvider : ILevelProvider {
        public string GetLevel(double point, List<Level> levels) {
            return levels.OrderBy(item => item.Elevation)
                .Where(item => item.Elevation < point)
                .FirstOrDefault(item=> Math.Abs(item.Elevation - point) > 4.92125984251969)?.Name;
        }
    }
}