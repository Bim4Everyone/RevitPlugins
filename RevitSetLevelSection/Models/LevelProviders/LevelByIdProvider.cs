using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.LevelProviders {
    public class LevelByIdProvider : ILevelProvider {
        public Level GetLevel(Element element, ICollection<Level> levels) {
            return element.Document.GetElement(element.LevelId) as Level;
        }
    }
}