using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitSplitMepCurve.Models.Exceptions;
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

        for(int i = 0; i < intersections.Count; i++) {
            var point = intersections[i];
            var newId = PlumbingUtils.BreakCurve(doc, _pipe.Id, point);
            var newPipe = (Pipe) doc.GetElement(newId);
            newSegments.Add(newPipe);

            var connectorSymbol = settings.ConnectorRoundSymbol
                                  ?? throw new CannotGetConnectorSymbolException();

            var connector1 = GetClosestConnector(_pipe, point);
            var connector2 = GetClosestConnector(newPipe, point);
            var fitting = InsertConnector(
                connectorSymbol,
                newPipe.PipeType,
                connector1,
                connector2);
            insertedConnectors.Add(fitting);
        }

        return new SplitResult(_pipe, newSegments, insertedConnectors, DisplacementElements);
    }
}
