using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;

using RevitSplitMepCurve.Models.Settings;

namespace RevitSplitMepCurve.Models.Splittable;

internal class SplittablePipe : SplittableElement {
    private readonly Pipe _pipe;

    public SplittablePipe(Pipe pipe, ICollection<DisplacementElement> displacementElements)
        : base(pipe, displacementElements) {
        _pipe = pipe;
    }

    public override SplitResult Split(ISplitSettings settings) {
        var doc = _pipe.Document;
        var intersections = GetIntersections(settings.Levels);

        var newSegments = new List<MEPCurve>();
        var insertedConnectors = new List<FamilyInstance>();
        var currentId = _pipe.Id;

        foreach(var (_, point) in intersections) {
            var newId = PlumbingUtils.BreakCurve(doc, currentId, point);
            var newPipe = (Pipe)doc.GetElement(newId);
            newSegments.Add(newPipe);

            var fitting = InsertRoundConnector(doc, settings.ConnectorRoundSymbol, point,
                (MEPCurve)doc.GetElement(currentId), newPipe);
            if(fitting is not null) {
                insertedConnectors.Add(fitting);
            }

            currentId = newId;
        }

        return new SplitResult(_pipe, newSegments, insertedConnectors, DisplacementElements);
    }

    private static FamilyInstance InsertRoundConnector(
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
