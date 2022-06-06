using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.Native;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class LevelNearestProvider : ILevelProvider {
        public string GetLevelName(double point, List<Level> levels) {
            return levels
                .OrderBy(item => Math.Abs(item.Elevation - point))
                .FirstOrDefault()?.Name;
        }
    }
}