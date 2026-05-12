using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using RevitSplitMepCurve.Models.Exceptions;
using RevitSplitMepCurve.Models.Settings;

namespace RevitSplitMepCurve.Models.Splittable;

internal class SplittableDuct : SplittableElement {
    private readonly Duct _duct;

    public SplittableDuct(Duct duct, ICollection<DisplacementElement> displacementElements)
        : base(duct, displacementElements) {
        _duct = duct;
    }

    public override SplitResult Split(ISplitSettings settings) {
        var doc = _duct.Document;
        var intersections = GetIntersections(settings.Levels);

        var newSegments = new List<MEPCurve>();
        var insertedConnectors = new List<FamilyInstance>();

        for(int i = 0; i < intersections.Count; i++) {
            var point = intersections[i];
            var newId = MechanicalUtils.BreakCurve(doc, _duct.Id, point);
            var newDuct = (Duct) doc.GetElement(newId);
            newSegments.Add(newDuct);

            FamilySymbol connectorSymbol;
            if(_duct.DuctType.Shape == ConnectorProfileType.Round) {
                connectorSymbol = settings.ConnectorRoundSymbol;
            } else if(_duct.DuctType.Shape == ConnectorProfileType.Rectangular) {
                connectorSymbol = settings.ConnectorRectangleSymbol;
            } else {
                throw new CannotGetConnectorSymbolException();
            }

            var connector1 = GetClosestConnector(_duct, point);
            var connector2 = GetClosestConnector(newDuct, point);
            var fitting = InsertConnector(
                connectorSymbol,
                newDuct.DuctType,
                connector1,
                connector2);
            insertedConnectors.Add(fitting);
        }

        return new SplitResult(_duct, newSegments, insertedConnectors, DisplacementElements);
    }
}
