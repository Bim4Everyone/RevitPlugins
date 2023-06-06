using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitSetLevelSection.Models.LevelProviders {
    public class LevelByIdProvider : ILevelProvider {
        public Level GetLevel(Element element, ICollection<Level> levels) {
            return element.Document.GetElement(element.LevelId) as Level;
        }

        public static bool IsValidElement(Element element) {
            return element is SpatialElement && element.LevelId.IsNotNull();
        }
    }
}