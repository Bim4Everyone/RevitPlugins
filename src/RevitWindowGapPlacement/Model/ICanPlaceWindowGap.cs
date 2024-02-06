using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal interface ICanPlaceWindowGap {
        List<FamilyInstance> PlaceWindowGap(Document document, FamilySymbol windowGapType);
    }
}