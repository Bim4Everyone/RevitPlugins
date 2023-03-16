using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.LevelDefinitions.BBPositions;
using RevitSetLevelSection.Models.LevelDefinitions.LevelProviders;

namespace RevitSetLevelSection.Models.LevelDefinitions {
    internal class LevelDefinition {
        public IBBPosition BBPosition { get; set; }
        public ILevelProvider LevelProvider { get; set; }

        public string GetLevelName(Element element, List<Level> levels) {
            double position = BBPosition.GetPosition(element);
            return LevelProvider.GetLevel(position, levels);
        }

        public static readonly IDictionary<ElementId, LevelDefinition> ARDefinitions =
            new Dictionary<ElementId, LevelDefinition>() {
                {
                    new ElementId(BuiltInCategory.OST_Walls),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                },
            };
        
        public static readonly IDictionary<ElementId, LevelDefinition> KRDefinitions =
            new Dictionary<ElementId, LevelDefinition>();
        
        public static readonly IDictionary<ElementId, LevelDefinition> VISDefinitions =
            new Dictionary<ElementId, LevelDefinition>();
    }
}