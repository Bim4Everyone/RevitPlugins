using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions.LevelProviders {
    internal interface ILevelProvider {
        string GetLevel(double point, List<Level> levels);
    }
}