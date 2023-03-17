using Autodesk.Revit.DB;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Factories {
    internal interface IElementPositionFactory {
        IElementPosition Create(Element element);
    }
}