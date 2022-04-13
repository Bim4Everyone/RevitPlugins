using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal interface ICanPlaceWindowGap {
        FamilyInstance PlaceWindowGap(Document document, FamilySymbol windowGapType);
    }
}