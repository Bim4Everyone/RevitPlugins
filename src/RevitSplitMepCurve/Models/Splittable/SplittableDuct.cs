using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;

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
        var currentId = _duct.Id;

        var shape = _duct.DuctType.Shape;
        if(shape == ConnectorProfileType.Oval) {
            throw new InvalidOperationException("Error.OvalNotSupported");
        }

        foreach(var (_, point) in intersections) {
            var newId = MechanicalUtils.BreakCurve(doc, currentId, point);
            var newDuct = (Duct)doc.GetElement(newId);
            newSegments.Add(newDuct);

            var symbol = shape == ConnectorProfileType.Round
                ? settings.ConnectorRoundSymbol
                : settings.ConnectorRectangleSymbol;

            if(symbol is null && shape != ConnectorProfileType.Round) {
                throw new InvalidOperationException("Error.NoRectangleConnector");
            }

            var fitting = InsertConnector(doc, symbol, point,
                (MEPCurve)doc.GetElement(currentId), newDuct);
            if(fitting is not null) {
                insertedConnectors.Add(fitting);
            }

            currentId = newId;
        }

        return new SplitResult(_duct, newSegments, insertedConnectors, DisplacementElements);
    }

    private static FamilyInstance InsertConnector(
        Document doc, FamilySymbol symbol, XYZ point, MEPCurve before, MEPCurve after) {
        if(symbol is null) {
            return null;
        }

        var fitting = doc.Create.NewFamilyInstance(point, symbol, StructuralType.NonStructural);
        AlignFittingToMepCurve(doc, fitting, before, point);
        ConnectFittingToCurves(fitting, before, after, point);
        return fitting;
    }

    private static void AlignFittingToMepCurve(Document doc, FamilyInstance fitting, MEPCurve curve, XYZ point) {
        var locationCurve = (LocationCurve)curve.Location;
        var line = locationCurve.Curve as Line;
        if(line is null) {
            return;
        }
        var direction = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
        var xAxis = XYZ.BasisX;
        if(direction.IsAlmostEqualTo(xAxis) || direction.IsAlmostEqualTo(xAxis.Negate())) {
            return;
        }
        var axis = Line.CreateUnbound(point, XYZ.BasisZ);
        double angle = Math.Atan2(direction.Y, direction.X);
        ElementTransformUtils.RotateElement(doc, fitting.Id, axis, angle);
    }
}
