using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class LevelDefinition {
        public IBBPosition BBPosition { get; set; }
        public ILevelProvider LevelProvider { get; set; }

        public string GetLevelName(Outline outline, List<Level> levels) {
            double position = BBPosition.GetPosition(outline);
            return LevelProvider.GetLevelName(position, levels);
        }
    }
}