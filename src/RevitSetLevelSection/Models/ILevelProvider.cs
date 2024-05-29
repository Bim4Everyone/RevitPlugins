using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models {
    internal interface ILevelProvider {
        Level GetLevel(Element element, ICollection<Level> levels);
    }
}