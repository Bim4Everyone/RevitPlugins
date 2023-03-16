using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.LevelDefinitions.BBPositions;

namespace RevitSetLevelSection.Models.LevelDefinitions.LevelProviders {
    internal class TopLevelProvider : ILevelProvider {
        public string GetLevel(double point, List<Level> levels) {
            return levels.OrderBy(item => item.Elevation)
                .Where(item => item.Elevation > point)
                .FirstOrDefault()?.Name;
        }
    }
}