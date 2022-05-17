using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal interface ILevelProvider {
        string GetLevelName(double point, List<Level> levels);
    }
}