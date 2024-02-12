using Autodesk.Revit.DB;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Factories {
    internal interface IElementPositionFactory {
        bool CanCreate(Element element);
        IElementPosition Create(Element element);
    }
}