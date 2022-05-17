using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.Native;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class LevelBottomProvider : ILevelProvider {
        public string GetLevelName(double point, List<Level> levels) {
            return levels
                .Select(item => new {
                    Level = item, Distance = Math.Abs(item.Elevation - point), Sign = (item.Elevation - point) < 0
                })
                .Where(item => item.Sign)
                .OrderBy(item => item.Distance)
                .Select(item => item.Level)
                .FirstOrDefault()?.Name;

        }
    }
}